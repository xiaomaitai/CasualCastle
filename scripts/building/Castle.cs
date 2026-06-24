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

	[Export]
	public int CellSize = 64;

	[Export]
	public int BlockSize = 60;

	[Export]
	public Color BlockColor = new Color(1, 1, 1, 0.06f);

	[Export]
	public Color AreaBgColor = new Color(1, 1, 1, 0.03f);

	[Export]
	public PackedScene BarracksScene;

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

	[Export]
	public float SpawnInset = 8f;

	public CastleHeart Heart { get; private set; }
	public bool IsAlive => Heart != null && Heart.Health > 0;

	private ProgressBar _healthBar;
	private bool[,] _occupied;
	private bool _showPlacementPreview;
	private int _previewGridX;
	private int _previewGridY;
	private bool _previewValid;
	private IReadOnlyList<Vector2I> _previewFootprint = BuildingSystem.GetFootprint("Barracks");
	private CastleHighlightOverlay _highlightOverlay;

	public override void _Ready()
	{
		_occupied = new bool[GridColumns, GridRows];
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

	public Vector2 GetCellCenter(int gridX, int gridY)
	{
		return new Vector2((gridX + 0.5f) * CellSize, (gridY + 0.5f) * CellSize);
	}

	public bool IsInBounds(int gridX, int gridY)
	{
		return gridX >= 0 && gridX < GridColumns && gridY >= 0 && gridY < GridRows;
	}

	public bool IsCellPassable(int gridX, int gridY)
	{
		return IsInBounds(gridX, gridY) && !_occupied[gridX, gridY];
	}

	public bool TryGetGridFromGlobalPoint(Vector2 globalPoint, out int gridX, out int gridY)
	{
		Vector2 localPoint = ToLocal(globalPoint);
		gridX = (int)(localPoint.X / CellSize);
		gridY = (int)(localPoint.Y / CellSize);
		return IsInBounds(gridX, gridY);
	}

	public bool CanPlaceFootprint(string buildingType, int anchorX, int anchorY)
	{
		return BuildingSystem.Instance?.CanPlace(this, buildingType, anchorX, anchorY) == true;
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
		if (!CanPlaceFootprint(buildingType, anchorX, anchorY))
			return false;

		foreach (Vector2I offset in footprint)
			_occupied[anchorX + offset.X, anchorY + offset.Y] = true;

		building.Position = GetFootprintCenter(anchorX, anchorY, footprint);
		AddChild(building);
		return true;
	}

	public void ReleaseBuildingFootprint(Building building)
	{
		if (building == null)
			return;

		IReadOnlyList<Vector2I> footprint = BuildingSystem.GetFootprint(building.TypeId);
		foreach (Vector2I offset in footprint)
		{
			int gridX = building.AnchorGridX + offset.X;
			int gridY = building.AnchorGridY + offset.Y;
			if (IsInBounds(gridX, gridY))
				_occupied[gridX, gridY] = false;
		}
	}

	public Vector2 GetFootprintCenter(int anchorX, int anchorY, IReadOnlyList<Vector2I> footprint)
	{
		Vector2 sum = Vector2.Zero;
		foreach (Vector2I offset in footprint)
			sum += GetCellCenter(anchorX + offset.X, anchorY + offset.Y);

		return sum / footprint.Count;
	}

	public Vector2 GetBuildingSpawnPosition(int gridX, int gridY, Vector2I outwardDir, int spawnIndex = 0)
	{
		int spawnGridX = gridX + outwardDir.X;
		int spawnGridY = gridY + outwardDir.Y;
		float spreadX = (spawnIndex % 3) * 10f;
		float spreadY = (spawnIndex % 3) * 8f;

		return new Vector2(
			spawnGridX * CellSize + SpawnInset + spreadX,
			(spawnGridY + 1) * CellSize - SpawnInset - spreadY
		);
	}

	private void SetupCastleHeart()
	{
		PackedScene scene = GD.Load<PackedScene>("res://prefabs/CastleHeart.tscn");
		if (scene == null)
			return;

		CastleHeart heart = scene.Instantiate<CastleHeart>();
		heart.InitFromType("CastleHeart");
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
		PackedScene scene = BarracksScene ?? GD.Load<PackedScene>("res://prefabs/Barracks.tscn");
		if (scene == null)
			return;

		Building building = scene.Instantiate<Building>();
		building.InitFromType("Barracks");
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
		int totalWidth = GridColumns * CellSize;
		int totalHeight = GridRows * CellSize;

		DrawRect(new Rect2(0, 0, totalWidth, totalHeight), AreaBgColor);

		int blockInset = (CellSize - BlockSize) / 2;

		for (int row = 0; row < GridRows; row++)
		{
			for (int col = 0; col < GridColumns; col++)
			{
				Vector2 position = new Vector2(col * CellSize + blockInset, row * CellSize + blockInset);
				DrawRect(new Rect2(position, new Vector2(BlockSize, BlockSize)), BlockColor);
			}
		}

		if (_showPlacementPreview)
		{
			Color previewColor = _previewValid
				? new Color(0.2f, 0.85f, 0.35f, 0.35f)
				: new Color(0.9f, 0.2f, 0.2f, 0.35f);

			foreach (Vector2I offset in _previewFootprint)
			{
				int col = _previewGridX + offset.X;
				int row = _previewGridY + offset.Y;
				if (!IsInBounds(col, row))
					continue;

				Vector2 previewPos = new Vector2(col * CellSize, row * CellSize);
				DrawRect(new Rect2(previewPos, new Vector2(CellSize, CellSize)), previewColor);
			}
		}
	}
}
