// 导航网格：战场寻路格子与阻塞标记
using System;

namespace CasualCastle.Domain.Battle;

public sealed class NavGrid
{
	public const float CellSize = 50f;

	public readonly int Width;
	public readonly int Height;
	public readonly float OriginX;
	public readonly float OriginY;
	private readonly bool[] _blocked;

	public NavGrid(int width, int height, float originX, float originY)
	{
		Width = width;
		Height = height;
		OriginX = originX;
		OriginY = originY;
		_blocked = new bool[width * height];
	}

	public int CellCount => Width * Height;

	public bool InBounds(int cx, int cy)
	{
		return cx >= 0 && cx < Width && cy >= 0 && cy < Height;
	}

	public int Index(int cx, int cy)
	{
		return cy * Width + cx;
	}

	public void SetBlocked(int cx, int cy, bool blocked)
	{
		_blocked[Index(cx, cy)] = blocked;
	}

	public bool IsBlockedCell(int cx, int cy)
	{
		return !InBounds(cx, cy) || _blocked[Index(cx, cy)];
	}

	public bool IsBlockedAtWorld(float wx, float wy)
	{
		return IsBlockedCell(WorldToCellX(wx), WorldToCellY(wy));
	}

	public int WorldToCellX(float wx)
	{
		return (int)Math.Floor((wx - OriginX) / CellSize);
	}

	public int WorldToCellY(float wy)
	{
		return (int)Math.Floor((wy - OriginY) / CellSize);
	}

	public float CellCenterX(int cx)
	{
		return OriginX + (cx + 0.5f) * CellSize;
	}

	public float CellCenterY(int cy)
	{
		return OriginY + (cy + 0.5f) * CellSize;
	}

	public void ClampToBounds(ref float wx, ref float wy)
	{
		wx = Math.Clamp(wx, OriginX, OriginX + Width * CellSize);
		wy = Math.Clamp(wy, OriginY, OriginY + Height * CellSize);
	}
}
