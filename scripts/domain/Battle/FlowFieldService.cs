// 流场服务：从目标格反向 8 连通 Dijkstra 扩散生成方向场
using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public sealed class FlowFieldService
{
	private const float DiagonalCost = 1.41421356f;
	private static readonly int[] StepX = { -1, 1, 0, 0, -1, 1, -1, 1 };
	private static readonly int[] StepY = { 0, 0, -1, 1, -1, -1, 1, 1 };
	private static readonly float[] StepCost = { 1f, 1f, 1f, 1f, DiagonalCost, DiagonalCost, DiagonalCost, DiagonalCost };

	public FlowField Compute(NavGrid grid, int goalIndex)
	{
		FlowField field = new FlowField(grid.CellCount);
		PriorityQueue<int, float> queue = new PriorityQueue<int, float>();
		field.Reachable[goalIndex] = true;
		queue.Enqueue(goalIndex, 0f);
		while (queue.Count > 0)
			Expand(queue, grid, field, queue.Dequeue());
		return field;
	}

	private static void Expand(PriorityQueue<int, float> queue, NavGrid grid, FlowField field, int index)
	{
		float cost = field.Cost[index];
		int cx = index % grid.Width;
		int cy = index / grid.Width;
		for (int i = 0; i < StepX.Length; i++)
			TryRelax(queue, grid, field, cx + StepX[i], cy + StepY[i], cost + StepCost[i], -StepX[i], -StepY[i]);
	}

	private static void TryRelax(PriorityQueue<int, float> queue, NavGrid grid, FlowField field, int nx, int ny, float newCost, int dirX, int dirY)
	{
		if (!grid.InBounds(nx, ny) || grid.IsBlockedCell(nx, ny))
			return;
		int ni = grid.Index(nx, ny);
		if (field.Reachable[ni] && field.Cost[ni] <= newCost)
			return;
		field.Reachable[ni] = true;
		field.Cost[ni] = newCost;
		field.DirX[ni] = dirX;
		field.DirY[ni] = dirY;
		queue.Enqueue(ni, newCost);
	}
}
