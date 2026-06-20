using Godot;
using System;

public partial class Castle : Node2D
{
	[Export]
	public bool IsPlayerCastle;

	[Export]
	public int MaxHealth = 100;

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
	public int BarracksGridX = 6;

	[Export]
	public int BarracksGridY = 4;

	[Export]
	public float SpawnInset = 8f;

	public int Health { get; private set; }

	private ProgressBar _healthBar;
	private bool[,] _occupied;
	private bool _showPlacementPreview;
	private int _previewGridX;
	private int _previewGridY;
	private bool _previewValid;

	public override void _Ready()
	{
		Health = MaxHealth;
		_occupied = new bool[GridColumns, GridRows];
		_healthBar = GetNodeOrNull<ProgressBar>("HealthBar");
		UpdateHealthBar();
		SetupBarracks();
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

	public void SetPlacementPreview(bool show, int gridX, int gridY, bool valid)
	{
		_showPlacementPreview = show;
		_previewGridX = gridX;
		_previewGridY = gridY;
		_previewValid = valid;
		QueueRedraw();
	}

	public void ClearPlacementPreview()
	{
		SetPlacementPreview(false, 0, 0, false);
	}

	public bool PlaceBuilding(Building building, int gridX, int gridY)
	{
		if (!IsInBounds(gridX, gridY) || _occupied[gridX, gridY])
			return false;

		_occupied[gridX, gridY] = true;
		building.Position = GetCellCenter(gridX, gridY);
		AddChild(building);
		return true;
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

	private void SetupBarracks()
	{
		PackedScene scene = BarracksScene ?? GD.Load<PackedScene>("res://prefabs/Barracks.tscn");
		if (scene == null) return;

		Barracks barracks = scene.Instantiate<Barracks>();
		barracks.BindToGrid(this, BarracksGridX, BarracksGridY);
		PlaceBuilding(barracks, BarracksGridX, BarracksGridY);
	}

	public void TakeDamage(int amount)
	{
		if (Health <= 0) return;

		Health = Math.Max(0, Health - amount);
		UpdateHealthBar();
		GameManager.Instance?.TakeDamage(IsPlayerCastle, amount);
	}

	public void ResetHealth()
	{
		Health = MaxHealth;
		UpdateHealthBar();
	}

	private void UpdateHealthBar()
	{
		if (_healthBar == null) return;
		_healthBar.MaxValue = MaxHealth;
		_healthBar.Value = Health;
	}

	public override void _Draw()
	{
		int totalWidth = GridColumns * CellSize;
		int totalHeight = GridRows * CellSize;

		DrawRect(new Rect2(0, 0, totalWidth, totalHeight), AreaBgColor);

		int offset = (CellSize - BlockSize) / 2;

		for (int row = 0; row < GridRows; row++)
		{
			for (int col = 0; col < GridColumns; col++)
			{
				Vector2 position = new Vector2(col * CellSize + offset, row * CellSize + offset);
				DrawRect(new Rect2(position, new Vector2(BlockSize, BlockSize)), BlockColor);
			}
		}

		if (!_showPlacementPreview || !IsInBounds(_previewGridX, _previewGridY))
			return;

		Color previewColor = _previewValid
			? new Color(0.2f, 0.85f, 0.35f, 0.35f)
			: new Color(0.9f, 0.2f, 0.2f, 0.35f);
		Vector2 previewPos = new Vector2(_previewGridX * CellSize, _previewGridY * CellSize);
		DrawRect(new Rect2(previewPos, new Vector2(CellSize, CellSize)), previewColor);
	}
}
