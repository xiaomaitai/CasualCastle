// 空间哈希：按格子收集单位，加速邻居查询
using System;
using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public sealed class NavSpatialHash
{
	private const float CellSize = 128f;
	private readonly Dictionary<long, List<IUnitHandle>> _cells = new();

	public NavSpatialHash(IReadOnlyList<IUnitHandle> units)
	{
		foreach (IUnitHandle unit in units)
			if (unit.IsAlive)
				Add(unit);
	}

	private void Add(IUnitHandle unit)
	{
		long key = Key(WorldToCell(unit.GameX), WorldToCell(unit.GameY));
		List<IUnitHandle> bucket;
		if (!_cells.TryGetValue(key, out bucket))
		{
			bucket = new List<IUnitHandle>();
			_cells[key] = bucket;
		}
		bucket.Add(unit);
	}

	public List<IUnitHandle> Query(float x, float y, float range)
	{
		List<IUnitHandle> result = new List<IUnitHandle>();
		int centerX = WorldToCell(x);
		int centerY = WorldToCell(y);
		int cellRange = (int)Math.Ceiling(range / CellSize);
		for (int ox = -cellRange; ox <= cellRange; ox++)
			for (int oy = -cellRange; oy <= cellRange; oy++)
				Collect(centerX + ox, centerY + oy, result);
		return result;
	}

	private void Collect(int cx, int cy, List<IUnitHandle> result)
	{
		List<IUnitHandle> bucket;
		if (!_cells.TryGetValue(Key(cx, cy), out bucket))
			return;
		result.AddRange(bucket);
	}

	private static int WorldToCell(float world)
	{
		return (int)Math.Floor(world / CellSize);
	}

	private static long Key(int cx, int cy)
	{
		return ((long)cx << 32) | (uint)cy;
	}
}
