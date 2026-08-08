// 城堡表现：地面绘制、放置预览与血量条
using CasualCastle.Domain.Shared;
using CasualCastle.Domain.Building;
using CasualCastle.Adapters.Godot;
using Godot;
using System.Collections.Generic;

public partial class Castle
{
	private static readonly string[] CellTexturePaths = {
		"res://assets/art/tiles/cell_dirt_01.png",
		"res://assets/art/tiles/cell_dirt_02.png",
		"res://assets/art/tiles/cell_dirt_03.png",
		"res://assets/art/tiles/cell_dirt_04.png",
	};

	private static readonly Color ValidPreviewColor = new Color(0.2f, 0.85f, 0.35f, 0.35f);

	private static readonly Color InvalidPreviewColor = new Color(0.9f, 0.2f, 0.2f, 0.35f);

	private ProgressBar _healthBar;

	private CastleHighlightOverlay _highlightOverlay;

	private Texture2D[] _cellTextures;

	private void LoadCellTextures()
	{
		_cellTextures = new Texture2D[CellTexturePaths.Length];
		for (int index = 0; index < CellTexturePaths.Length; index++)
			_cellTextures[index] = GD.Load<Texture2D>(CellTexturePaths[index]);
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

	public void SetPlacementPreview(bool show, int gridX, int gridY, bool valid, string buildingType = BuildingTypeIds.Barracks)
	{
		_showPlacementPreview = show;
		_previewGridX = gridX;
		_previewGridY = gridY;
		_previewValid = valid;
		_previewFootprint = _buildingSystem.GetFootprint(buildingType);
		QueueRedraw();
	}

	public void ClearPlacementPreview()
	{
		SetPlacementPreview(false, 0, 0, false);
	}

	public void SetAdjacencyHighlightBuildings(IEnumerable<Building> buildings)
	{
		_highlightOverlay?.SetBuildings(buildings);
	}

	public void ClearAdjacencyHighlight()
	{
		_highlightOverlay?.ClearHighlights();
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
		DrawCellTextures();
		DrawPlacementPreview();
	}

	private void DrawCellTextures()
	{
		Vector2 cellPixelSize = GameCoordinatesAdapter.ToLocalPixels(
			GameCoordinateRules.UnitsPerCell,
			GameCoordinateRules.UnitsPerCell);
		for (int row = 0; row < GridRows; row++)
			for (int col = 0; col < GridColumns; col++)
			{
				Vector2 position = GameCoordinatesAdapter.ToLocalPixels(GameCoordinateRules.CellCorner(col, row));
				Texture2D texture = _cellTextures[(col + row) % _cellTextures.Length];
				DrawTextureRect(texture, new Rect2(position, cellPixelSize), false);
			}
	}

	private void DrawPlacementPreview()
	{
		if (!_showPlacementPreview)
			return;

		Color previewColor = _previewValid ? ValidPreviewColor : InvalidPreviewColor;
		Vector2 cellPixelSize = GameCoordinatesAdapter.ToLocalPixels(
			GameCoordinateRules.UnitsPerCell,
			GameCoordinateRules.UnitsPerCell);

		foreach (Vector2I offset in _previewFootprint)
			DrawPreviewCell(offset, cellPixelSize, previewColor);
	}

	private void DrawPreviewCell(Vector2I offset, Vector2 cellPixelSize, Color previewColor)
	{
		int col = _previewGridX + offset.X;
		int row = _previewGridY + offset.Y;
		if (!IsInBounds(col, row))
			return;

		Vector2 previewPos = GameCoordinatesAdapter.ToLocalPixels(GameCoordinateRules.CellCorner(col, row));
		DrawRect(new Rect2(previewPos, cellPixelSize), previewColor);
	}

}
