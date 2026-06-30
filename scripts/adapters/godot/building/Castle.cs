using CasualCastle.Domain.Shared;
using CasualCastle.Domain.Building;
using CasualCastle.Adapters.Godot;
using Godot;
using System;
using System.Collections.Generic;

public partial class Castle : Node2D
{
	[Export]
	public bool IsPlayerCastle;

	[Export]
	public int GridColumns = 8;

	[Export]
	public int GridRows = 8;

	public int CellSize => GameCoordinatesAdapter.PixelsPerCell;

	[Export]
	public Color BlockColor = new Color(1, 1, 1, 0.06f);

	[Export]
	public Color AreaBgColor = new Color(1, 1, 1, 0.03f);

	[Export]
	public int CastleHeartGridX = 3;

	[Export]
	public int CastleHeartGridY = 3;

	[Export]
	public int BarracksGridX = 6;

	[Export]
	public int BarracksGridY = 4;

	[Export]
	public int SecondBarracksGridX = -1;

	[Export]
	public int SecondBarracksGridY = -1;

	public Building Heart { get; private set; }
	public bool IsAlive => Heart != null && Heart.Health > 0;

	private ProgressBar _healthBar;
	private OccupancyGrid _occupancy;
	private bool _showPlacementPreview;
	private int _previewGridX;
	private int _previewGridY;
	private bool _previewValid;
	private IReadOnlyList<Vector2I> _previewFootprint = BuildingSystem.GetFootprint("Barracks");
	private CastleHighlightOverlay _highlightOverlay;

	public override void _Ready()
	{
		_occupancy = new OccupancyGrid(GridColumns, GridRows);
		_healthBar = GetNodeOrNull<ProgressBar>("HealthBar");
		SetupHighlightOverlay();
		SetupCastleHeart();
		SetupBarracks();
		UpdateHealthBar();
	}

	private void SetupHighlightOverlay()
	{
		_highlightOverlay = new CastleHighlightOverlay
		{
			ZIndex = 15,
		};
		_highlightOverlay.Bind(this);
		AddChild(_highlightOverlay);
	}

	public Vector2 GetCellCenter(int gridX, int gridY) =>
		GameCoordinatesAdapter.ToLocalPixels(GameCoordinateRules.CellCenter(gridX, gridY));

	public bool IsInBounds(int gridX, int gridY)
	{
		return gridX >= 0 && gridX < GridColumns && gridY >= 0 && gridY < GridRows;
	}

	public bool IsCellPassable(int gridX, int gridY)
	{
		return _occupancy.IsCellPassable(gridX, gridY);
	}

	public bool TryGetGridFromGlobalPoint(Vector2 globalPoint, out int gridX, out int gridY)
	{
		Vector2I grid = GameCoordinatesAdapter.FloorGridFromLocalPixels(ToLocal(globalPoint));
		gridX = grid.X;
		gridY = grid.Y;
		return IsInBounds(gridX, gridY);
	}

	public bool CanPlaceFootprint(string buildingType, int anchorX, int anchorY)
	{
		IReadOnlyList<GridCellOffset> footprint = BuildingDefinitions.GetFootprint(buildingType);
		return _occupancy.CanPlaceFootprint(footprint, anchorX, anchorY);
	}

	public List<Building> GetBuildings()
	{
		List<Building> buildings = new();
		foreach (Node child in GetChildren())
		{
			if (child is Building building)
				buildings.Add(building);
		}

		return buildings;
	}

	public List<IBuildingState> GetBuildingStates()
	{
		List<IBuildingState> states = new();
		foreach (Node child in GetChildren())
		{
			if (child is IBuildingState state)
				states.Add(state);
		}

		return states;
	}

	public void SetPlacementPreview(bool show, int gridX, int gridY, bool valid, string buildingType = "Barracks")
	{
		_showPlacementPreview = show;
		_previewGridX = gridX;
		_previewGridY = gridY;
		_previewValid = valid;
		_previewFootprint = BuildingSystem.GetFootprint(buildingType);
		QueueRedraw();
	}

	public void ClearPlacementPreview()
	{
		SetPlacementPreview(false, 0, 0, false);
	}

	public bool TryGetBuildingAtGlobalPoint(Vector2 globalPoint, out Building building)
	{
		building = null;
		if (!TryGetGridFromGlobalPoint(globalPoint, out int gridX, out int gridY))
			return false;

		foreach (Building candidate in GetBuildings())
		{
			foreach (Vector2I offset in BuildingSystem.GetFootprint(candidate.TypeId))
			{
				if (candidate.AnchorGridX + offset.X != gridX || candidate.AnchorGridY + offset.Y != gridY)
					continue;

				building = candidate;
				return true;
			}
		}

		return false;
	}

	public void SetAdjacencyHighlightBuildings(IEnumerable<Building> buildings)
	{
		_highlightOverlay?.SetBuildings(buildings);
	}

	public void ClearAdjacencyHighlight()
	{
		_highlightOverlay?.ClearHighlights();
	}

	public bool PlaceBuilding(Building building, int gridX, int gridY)
	{
		return PlaceBuilding(building, gridX, gridY, "Barracks");
	}

	public bool PlaceBuilding(Building building, int anchorX, int anchorY, string buildingType)
	{
		IReadOnlyList<Vector2I> footprint = BuildingSystem.GetFootprint(buildingType);
		IReadOnlyList<GridCellOffset> domainFootprint = BuildingDefinitions.GetFootprint(buildingType);
		if (!CanPlaceFootprint(buildingType, anchorX, anchorY))
			return false;

		_occupancy.OccupyCells(domainFootprint, anchorX, anchorY);

		building.Position = GetFootprintCenter(anchorX, anchorY, footprint);
		AddChild(building);
		return true;
	}

	public void ReleaseBuildingFootprint(Building building)
	{
		if (building == null)
			return;

		IReadOnlyList<GridCellOffset> footprint = BuildingDefinitions.GetFootprint(building.TypeId);
		_occupancy.ReleaseCells(footprint, building.AnchorGridX, building.AnchorGridY);
	}

	public Vector2 GetFootprintCenter(int anchorX, int anchorY, IReadOnlyList<Vector2I> footprint)
	{
		Vector2 sum = Vector2.Zero;
		foreach (Vector2I offset in footprint)
			sum += GetCellCenter(anchorX + offset.X, anchorY + offset.Y);

		return sum / footprint.Count;
	}

	public Vector2 GetBuildingSpawnGlobalPosition(
		IReadOnlyList<Vector2I> footprint, int anchorX, int anchorY, int spawnIndex = 0) =>
		UnitSpawn.GetSpawnGlobalPosition(this, footprint, anchorX, anchorY, spawnIndex);

	public Vector2 GetBuildingSpawnPosition(IReadOnlyList<Vector2I> footprint, int anchorX, int anchorY, int spawnIndex = 0) =>
		ToLocal(GetBuildingSpawnGlobalPosition(footprint, anchorX, anchorY, spawnIndex));

	public bool IsAnyCellOccupiedByPlayerSoldier(IReadOnlyList<Vector2I> cells)
	{
		if (cells == null || cells.Count == 0)
			return false;

		Node2D battlefield = GetNodeOrNull<Node2D>("/root/MainGame/Battlefield")
			?? GetTree().Root.GetNodeOrNull<Node2D>("MainGame/Battlefield");
		if (battlefield == null)
			return false;

		foreach (Node child in battlefield.GetChildren())
		{
			if (child is not SoldierLogic soldier || !soldier.IsAlive || !soldier.IsPlayerUnit)
				continue;

			if (!TryGetGridFromGlobalPoint(soldier.GlobalPosition, out int gridX, out int gridY))
				continue;

			foreach (Vector2I cell in cells)
			{
				if (cell.X == gridX && cell.Y == gridY)
					return true;
			}
		}

		return false;
	}

	private void SetupCastleHeart()
	{
		Building heart = BuildingSystem.CreateBuilding("CastleHeart");
		if (heart == null)
			return;

		heart.BindToGrid(this, CastleHeartGridX, CastleHeartGridY);
		PlaceBuilding(heart, CastleHeartGridX, CastleHeartGridY, "CastleHeart");
		Heart = heart;
		heart.HealthChanged += OnHeartHealthChanged;
	}

	private void SetupBarracks()
	{
		PlaceInitialBarracks(BarracksGridX, BarracksGridY);
		if (SecondBarracksGridX >= 0 && SecondBarracksGridY >= 0)
			PlaceInitialBarracks(SecondBarracksGridX, SecondBarracksGridY);
	}

	private void PlaceInitialBarracks(int gridX, int gridY)
	{
		Building building = BuildingSystem.CreateBuilding("Barracks");
		if (building == null)
			return;

		building.BindToGrid(this, gridX, gridY);
		PlaceBuilding(building, gridX, gridY);
	}

	private void OnHeartHealthChanged(int health, int maxHealth)
	{
		UpdateHealthBar();
	}

	private void UpdateHealthBar()
	{
		if (_healthBar == null || Heart == null)
			return;

		_healthBar.MaxValue = Heart.MaxHealth;
		_healthBar.Value = Heart.Health;
	}

	public override void _Draw()
	{
		int unitsPerCell = GameCoordinateRules.UnitsPerCell;
		Vector2 gridPixelSize = GameCoordinatesAdapter.ToLocalPixels(
			GridColumns * unitsPerCell,
			GridRows * unitsPerCell);
		DrawRect(new Rect2(Vector2.Zero, gridPixelSize), AreaBgColor);

		Vector2 blockPixelSize = GameCoordinatesAdapter.ToLocalPixels(
			GameCoordinateRules.CellBlockSize,
			GameCoordinateRules.CellBlockSize);

		for (int row = 0; row < GridRows; row++)
		{
			for (int col = 0; col < GridColumns; col++)
			{
				Vector2 position = GameCoordinatesAdapter.ToLocalPixels(GameCoordinateRules.CellBlockOrigin(col, row));
				DrawRect(new Rect2(position, blockPixelSize), BlockColor);
			}
		}

		if (_showPlacementPreview)
		{
			Color previewColor = _previewValid
				? new Color(0.2f, 0.85f, 0.35f, 0.35f)
				: new Color(0.9f, 0.2f, 0.2f, 0.35f);

			Vector2 cellPixelSize = GameCoordinatesAdapter.ToLocalPixels(
				unitsPerCell,
				unitsPerCell);

			foreach (Vector2I offset in _previewFootprint)
			{
				int col = _previewGridX + offset.X;
				int row = _previewGridY + offset.Y;
				if (!IsInBounds(col, row))
					continue;

				Vector2 previewPos = GameCoordinatesAdapter.ToLocalPixels(GameCoordinateRules.CellCorner(col, row));
				DrawRect(new Rect2(previewPos, cellPixelSize), previewColor);
			}
		}
	}
}
