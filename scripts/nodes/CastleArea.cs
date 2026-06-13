using Godot;
using System;

public partial class CastleArea : Node2D
{
    /// <summary>
    /// 城堡的网格列数（水平方向地块数）
    /// </summary>
    [Export]
    public int GridColumns = 8;

    /// <summary>
    /// 城堡的网格行数（垂直方向地块数）
    /// </summary>
    [Export]
    public int GridRows = 8;

    /// <summary>
    /// 每个地块的像素大小
    /// </summary>
    [Export]
    public int CellSize = 64;

    /// <summary>
    /// 地块内方块的大小（略小于 CellSize 以形成网格线）
    /// </summary>
    [Export]
    public int BlockSize = 60;

    /// <summary>
    /// 方块颜色（半透明白）
    /// </summary>
    [Export]
    public Color BlockColor = new Color(1, 1, 1, 0.06f);

    /// <summary>
    /// 城堡区域背景色
    /// </summary>
    [Export]
    public Color AreaBgColor = new Color(1, 1, 1, 0.03f);

    public override void _Draw()
    {
        int totalWidth = GridColumns * CellSize;
        int totalHeight = GridRows * CellSize;

        // 绘制城堡区域背景
        DrawRect(new Rect2(0, 0, totalWidth, totalHeight), AreaBgColor);

        // 偏移量使方块居中于每个格子（(CellSize - BlockSize) / 2）
        int offset = (CellSize - BlockSize) / 2;

        // 绘制每个地块的方块
        for (int row = 0; row < GridRows; row++)
        {
            for (int col = 0; col < GridColumns; col++)
            {
                Vector2 position = new Vector2(col * CellSize + offset, row * CellSize + offset);
                DrawRect(new Rect2(position, new Vector2(BlockSize, BlockSize)), BlockColor);
            }
        }
    }
}