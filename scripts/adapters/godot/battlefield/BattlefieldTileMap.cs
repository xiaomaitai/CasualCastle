// 使用 Kenney 图集生成战场地面、道路与环境装饰。
using Godot;

public partial class BattlefieldTileMap : Node2D
{
	private const string TilesheetPath = "res://assets/art/tiles/kenney_medieval_rts/medieval_tilesheet.png";
	private const int AtlasSourceId = 0;
	private const int AtlasTileSize = 64;
	private const int WorldTileSize = 100;
	private const int AtlasMargin = 32;
	private const int AtlasSeparation = 32;
	private const int GroundVariantModulo = 23;
	private const int FlowerVariantThreshold = 2;
	private const int TopLaneOffset = 1;
	private const int MiddleLaneOffset = 4;
	private const int BottomLaneOffset = 7;
	private const int CenterPathMinRow = 0;
	private const int ForestStartRow = 1;
	private const int ForestColumnInset = 1;
	private const int MeadowColumnInset = 2;
	private const int ForestMaxRow = 8;
	private const int ForestSpacing = 7;
	private const int MeadowDetailMinRow = ForestMaxRow;
	private const int MeadowDetailSpacing = 19;
	private const int HashMultiplierX = 73856093;
	private const int HashMultiplierY = 19349663;
	private const float MapOriginX = BattlefieldMetrics.HorizontalSideMargin - WorldTileSize;
	private const float MapOriginY = BattlefieldMetrics.SideY % WorldTileSize;
	private const float TileMapScale = (float)WorldTileSize / AtlasTileSize;
	private static readonly Vector2I GrassTileA = new Vector2I(0, 0);
	private static readonly Vector2I GrassTileB = new Vector2I(1, 0);
	private static readonly Vector2I FlowerTileA = new Vector2I(0, 5);
	private static readonly Vector2I FlowerTileB = new Vector2I(1, 5);
	private static readonly Vector2I HorizontalPathTile = new Vector2I(5, 0);
	private static readonly Vector2I VerticalPathTile = new Vector2I(4, 0);
	private static readonly Vector2I CrossPathTile = new Vector2I(6, 0);
	private static readonly Vector2I[] TreeTiles = {
		new Vector2I(4, 3),
		new Vector2I(5, 3),
		new Vector2I(6, 3),
		new Vector2I(7, 3),
	};
	private static readonly Vector2I[] RockTiles = {
		new Vector2I(4, 4),
		new Vector2I(5, 4),
		new Vector2I(6, 4),
		new Vector2I(7, 4),
		new Vector2I(8, 4),
	};
	private TileMapLayer _groundLayer;
	private TileMapLayer _detailLayer;

	public override void _Ready()
	{
		Position = new Vector2(MapOriginX, MapOriginY);
		Scale = new Vector2(TileMapScale, TileMapScale);
		_groundLayer = GetNode<TileMapLayer>("GroundLayer");
		_detailLayer = GetNode<TileMapLayer>("DetailLayer");
		TileSet tileSet = CreateTileSet();
		_groundLayer.TileSet = tileSet;
		_detailLayer.TileSet = tileSet;
		DrawMap();
	}

	private TileSet CreateTileSet()
	{
		TileSet tileSet = new TileSet();
		tileSet.TileSize = new Vector2I(AtlasTileSize, AtlasTileSize);
		TileSetAtlasSource atlasSource = CreateAtlasSource();
		CreateAtlasTiles(atlasSource);
		tileSet.AddSource(atlasSource, AtlasSourceId);
		return tileSet;
	}

	private TileSetAtlasSource CreateAtlasSource()
	{
		TileSetAtlasSource atlasSource = new TileSetAtlasSource();
		atlasSource.Texture = GD.Load<Texture2D>(TilesheetPath);
		atlasSource.TextureRegionSize = new Vector2I(AtlasTileSize, AtlasTileSize);
		atlasSource.Margins = new Vector2I(AtlasMargin, AtlasMargin);
		atlasSource.Separation = new Vector2I(AtlasSeparation, AtlasSeparation);
		return atlasSource;
	}

	private void CreateAtlasTiles(TileSetAtlasSource atlasSource)
	{
		Vector2I[] groundTiles = {
			GrassTileA, GrassTileB, FlowerTileA, FlowerTileB,
			HorizontalPathTile, VerticalPathTile, CrossPathTile,
		};
		foreach (Vector2I tile in groundTiles)
			atlasSource.CreateTile(tile);
		foreach (Vector2I tile in TreeTiles)
			atlasSource.CreateTile(tile);
		foreach (Vector2I tile in RockTiles)
			atlasSource.CreateTile(tile);
	}

	private void DrawMap()
	{
		DrawGround();
		DrawRoads();
		DrawForest();
		DrawMeadowDetails();
	}

	private void DrawGround()
	{
		for (int row = MinRow(); row <= MaxRow(); row++)
			for (int column = MinColumn(); column <= MaxColumn(); column++)
				SetGround(column, row, SelectGrassTile(column, row));
	}

	private Vector2I SelectGrassTile(int column, int row)
	{
		int variant = PositiveHash(column, row) % GroundVariantModulo;
		if (variant == 0)
			return FlowerTileA;
		if (variant < FlowerVariantThreshold)
			return FlowerTileB;
		return variant % 2 == 0 ? GrassTileA : GrassTileB;
	}

	private int MinColumn()
	{
		return Mathf.FloorToInt(-MapOriginX / WorldTileSize);
	}

	private int MaxColumn()
	{
		return Mathf.CeilToInt((BattlefieldMetrics.WorldWidth - MapOriginX) / WorldTileSize) - 1;
	}

	private int MinRow()
	{
		return Mathf.FloorToInt(-MapOriginY / WorldTileSize);
	}

	private int MaxRow()
	{
		return Mathf.CeilToInt((BattlefieldMetrics.WorldHeight - MapOriginY) / WorldTileSize) - 1;
	}

	private void DrawRoads()
	{
		int castleTopRow = WorldToRow(BattlefieldMetrics.SideY);
		int[] laneRows = {
			castleTopRow + TopLaneOffset,
			castleTopRow + MiddleLaneOffset,
			castleTopRow + BottomLaneOffset,
		};
		foreach (int row in laneRows)
			DrawHorizontalRoad(row);
		DrawVerticalRoad(laneRows);
	}

	private void DrawHorizontalRoad(int row)
	{
		int minColumn = WorldToColumn(BattlefieldMetrics.PlayerSideX + BattlefieldMetrics.CastleGridPixels);
		int maxColumn = WorldToColumn(BattlefieldMetrics.EnemySideX);
		int centerColumn = WorldToColumn(BattlefieldMetrics.WorldWidth / 2f);
		for (int column = minColumn; column <= maxColumn; column++)
			SetGround(column, row, column == centerColumn ? CrossPathTile : HorizontalPathTile);
	}

	private void DrawVerticalRoad(int[] laneRows)
	{
		int centerColumn = WorldToColumn(BattlefieldMetrics.WorldWidth / 2f);
		int maxRow = laneRows[laneRows.Length - 1];
		for (int row = CenterPathMinRow; row <= maxRow; row++)
			SetGround(centerColumn, row, IsLaneRow(row, laneRows) ? CrossPathTile : VerticalPathTile);
	}

	private bool IsLaneRow(int row, int[] laneRows)
	{
		foreach (int laneRow in laneRows)
			if (row == laneRow)
				return true;
		return false;
	}

	private void DrawForest()
	{
		int centerColumn = WorldToColumn(BattlefieldMetrics.WorldWidth / 2f);
		for (int row = ForestStartRow; row < ForestMaxRow; row++)
			for (int column = MinColumn() + ForestColumnInset; column < MaxColumn(); column++)
				if (PositiveHash(column, row) % ForestSpacing == 0 && column != centerColumn)
					SetDetail(column, row, SelectDetailTile(TreeTiles, column, row));
	}

	private void DrawMeadowDetails()
	{
		int maxRow = WorldToRow(BattlefieldMetrics.SideY) - 1;
		int centerColumn = WorldToColumn(BattlefieldMetrics.WorldWidth / 2f);
		for (int row = MeadowDetailMinRow; row <= maxRow; row++)
			for (int column = MinColumn() + MeadowColumnInset; column < MaxColumn() - MeadowColumnInset; column++)
				if (PositiveHash(column, row) % MeadowDetailSpacing == 0 && column != centerColumn)
					SetDetail(column, row, SelectDetailTile(RockTiles, column, row));
	}

	private int WorldToColumn(float worldX)
	{
		return Mathf.FloorToInt((worldX - MapOriginX) / WorldTileSize);
	}

	private int WorldToRow(float worldY)
	{
		return Mathf.FloorToInt((worldY - MapOriginY) / WorldTileSize);
	}

	private Vector2I SelectDetailTile(Vector2I[] tiles, int column, int row)
	{
		int index = PositiveHash(column + row, row - column) % tiles.Length;
		return tiles[index];
	}

	private int PositiveHash(int column, int row)
	{
		int hash = column * HashMultiplierX ^ row * HashMultiplierY;
		return hash & int.MaxValue;
	}

	private void SetGround(int column, int row, Vector2I atlasCoordinates)
	{
		_groundLayer.SetCell(new Vector2I(column, row), AtlasSourceId, atlasCoordinates);
	}

	private void SetDetail(int column, int row, Vector2I atlasCoordinates)
	{
		_detailLayer.SetCell(new Vector2I(column, row), AtlasSourceId, atlasCoordinates);
	}
}
