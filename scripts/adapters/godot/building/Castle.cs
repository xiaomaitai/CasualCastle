// 城堡核心：网格占用、放置校验与建筑查询
using CasualCastle.Domain.Shared;
using CasualCastle.Domain.Building;
using CasualCastle.Adapters.Godot;
using Godot;
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

	private OccupancyGrid _occupancy;

	private bool _showPlacementPreview;

	private int _previewGridX;

	private int _previewGridY;

	private bool _previewValid;

	private IReadOnlyList<Vector2I> _previewFootprint;

	private BuildingSystem _buildingSystem;

	public override void _Ready()
	{
		_buildingSystem = AdapterRegistry.Resolve<BuildingSystem>();
		_previewFootprint = _buildingSystem.GetFootprint(BuildingTypeIds.Barracks);
		_occupancy = new OccupancyGrid(GridColumns, GridRows);
		_healthBar = GetNodeOrNull<ProgressBar>("HealthBar");
		LoadCellTextures();
		SetupHighlightOverlay();
		SetupCastleHeart();
		SetupBarracks();
		UpdateHealthBar();
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

	public bool TryGetGridFromScreenPoint(Vector2 screenPoint, out int gridX, out int gridY)
	{
		Vector2 worldPoint = GameCoordinatesAdapter.ScreenToWorld(GetViewport(), screenPoint);
		return TryGetGridFromGlobalPoint(worldPoint, out gridX, out gridY);
	}

	public bool TryGetBuildingAtScreenPoint(Vector2 screenPoint, out Building building)
	{
		Vector2 worldPoint = GameCoordinatesAdapter.ScreenToWorld(GetViewport(), screenPoint);
		return TryGetBuildingAtGlobalPoint(worldPoint, out building);
	}

	public bool CanPlaceFootprint(string buildingType, int anchorX, int anchorY)
	{
		IReadOnlyList<GridCellOffset> footprint = GameManager.Get<IBuildingRepository>().GetFootprint(buildingType);
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

	public bool TryGetBuildingAtGlobalPoint(Vector2 globalPoint, out Building building)
	{
		building = null;
		if (!TryGetGridFromGlobalPoint(globalPoint, out int gridX, out int gridY))
			return false;

		foreach (Building candidate in GetBuildings())
		{
			foreach (Vector2I offset in _buildingSystem.GetFootprint(candidate.TypeId))
			{
				if (candidate.AnchorGridX + offset.X != gridX || candidate.AnchorGridY + offset.Y != gridY)
					continue;

				building = candidate;
				return true;
			}
		}

		return false;
	}

	public bool PlaceBuilding(Building building, int gridX, int gridY)
	{
		return PlaceBuilding(building, gridX, gridY, BuildingTypeIds.Barracks);
	}

	public bool PlaceBuilding(Building building, int anchorX, int anchorY, string buildingType)
	{
		IReadOnlyList<Vector2I> footprint = _buildingSystem.GetFootprint(buildingType);
		IReadOnlyList<GridCellOffset> domainFootprint = GameManager.Get<IBuildingRepository>().GetFootprint(buildingType);
		if (!CanPlaceFootprint(buildingType, anchorX, anchorY))
			return false;

		_occupancy.OccupyCells(domainFootprint, anchorX, anchorY);

		building.Position = GetFootprintCenter(anchorX, anchorY, footprint);
		AddChild(building);
		return true;
	}

	public void ClearNonCoreBuildings()
	{
		List<Building> buildings = GetBuildings();
		foreach (Building b in buildings)
		{
			if (_buildingSystem.IsCoreBuilding(b.TypeId))
				continue;
			ReleaseBuildingFootprint(b);
			b.GetParent()?.RemoveChild(b);
			b.QueueFree();
		}
	}

	public void ReleaseBuildingFootprint(Building building)
	{
		if (building == null)
			return;

		IReadOnlyList<GridCellOffset> footprint = GameManager.Get<IBuildingRepository>().GetFootprint(building.TypeId);
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

	public bool IsAnyCellOccupiedByPlayerUnit(IReadOnlyList<Vector2I> cells)
	{
		if (cells == null || cells.Count == 0)
			return false;

		Node2D battlefield = GetNodeOrNull<Node2D>("/root/MainGame/Battlefield")
			?? GetTree().Root.GetNodeOrNull<Node2D>("MainGame/Battlefield");
		if (battlefield == null)
			return false;

		foreach (Node child in battlefield.GetChildren())
		{
			if (child is not UnitLogic unit || !unit.IsAlive || !unit.IsPlayerUnit)
				continue;

			if (!TryGetGridFromGlobalPoint(unit.GlobalPosition, out int gridX, out int gridY))
				continue;

			foreach (Vector2I cell in cells)
			{
				if (cell.X == gridX && cell.Y == gridY)
					return true;
			}
		}

		return false;
	}

}
