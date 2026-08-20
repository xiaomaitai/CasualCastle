// 转向服务：流场跟随、软分离、到达刹车与贴墙滑动
using System;
using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public sealed class SteeringService
{
	public const float SeparationClearance = 4f;
	private const float ArrivalRadius = 8f;
	private const float SeparationScale = 0.6f;
	private const float VelocitySmoothing = 12f;
	private const float Epsilon = 0.001f;

	public void Steer(IUnitHandle unit, NavGrid grid, FlowField field, IReadOnlyList<IUnitHandle> neighbors, float dt)
	{
		if (DistanceToGoal(unit) <= ArrivalRadius)
		{
			unit.SetNavVelocity(0f, 0f);
			return;
		}
		float desiredX, desiredY;
		ComputeDesired(unit, grid, field, neighbors, out desiredX, out desiredY);
		float blend = Math.Min(1f, VelocitySmoothing * dt);
		float vx = unit.NavVelX + (desiredX - unit.NavVelX) * blend;
		float vy = unit.NavVelY + (desiredY - unit.NavVelY) * blend;
		ApplyWallSlide(unit, grid, ref vx, ref vy, dt);
		unit.SetNavVelocity(vx, vy);
		float nx = unit.GameX + vx * dt;
		float ny = unit.GameY + vy * dt;
		grid.ClampToBounds(ref nx, ref ny);
		unit.SetPosition(nx, ny);
	}

	private static float DistanceToGoal(IUnitHandle unit)
	{
		float dx = unit.MoveGoalX - unit.GameX;
		float dy = unit.MoveGoalY - unit.GameY;
		return MathF.Sqrt(dx * dx + dy * dy);
	}

	private void ComputeDesired(IUnitHandle unit, NavGrid grid, FlowField field, IReadOnlyList<IUnitHandle> neighbors, out float desiredX, out float desiredY)
	{
		float speed = unit.GetCurrentSpeed();
		float flowX, flowY;
		SampleFlow(unit, grid, field, out flowX, out flowY);
		if (flowX == 0f && flowY == 0f)
			FlowTowardGoal(unit, out flowX, out flowY);
		float sepX, sepY;
		SampleSeparation(unit, neighbors, out sepX, out sepY);
		float tx = flowX + sepX * SeparationScale;
		float ty = flowY + sepY * SeparationScale;
		float length = MathF.Sqrt(tx * tx + ty * ty);
		if (length <= Epsilon)
		{
			desiredX = 0f;
			desiredY = 0f;
			return;
		}
		desiredX = tx / length * speed;
		desiredY = ty / length * speed;
	}

	private static void SampleFlow(IUnitHandle unit, NavGrid grid, FlowField field, out float flowX, out float flowY)
	{
		float gx = (unit.GameX - grid.OriginX) / NavGrid.CellSize - 0.5f;
		float gy = (unit.GameY - grid.OriginY) / NavGrid.CellSize - 0.5f;
		int cx = (int)Math.Floor(gx);
		int cy = (int)Math.Floor(gy);
		float tx = gx - cx;
		float ty = gy - cy;
		float ax, ay, bx, by, cx0, cy0;
		SampleCellDir(grid, field, cx, cy, out cx0, out cy0);
		SampleCellDir(grid, field, cx + 1, cy, out bx, out by);
		ax = cx0 + (bx - cx0) * tx;
		ay = cy0 + (by - cy0) * tx;
		SampleCellDir(grid, field, cx, cy + 1, out cx0, out cy0);
		SampleCellDir(grid, field, cx + 1, cy + 1, out bx, out by);
		bx = cx0 + (bx - cx0) * tx;
		by = cy0 + (by - cy0) * tx;
		Normalize(ax + (bx - ax) * ty, ay + (by - ay) * ty, out flowX, out flowY);
	}

	private static void SampleCellDir(NavGrid grid, FlowField field, int cx, int cy, out float dirX, out float dirY)
	{
		if (grid.IsBlockedCell(cx, cy))
		{
			dirX = 0f;
			dirY = 0f;
			return;
		}
		int index = grid.Index(cx, cy);
		if (!field.Reachable[index])
		{
			dirX = 0f;
			dirY = 0f;
			return;
		}
		dirX = field.DirX[index];
		dirY = field.DirY[index];
	}

	private static void FlowTowardGoal(IUnitHandle unit, out float flowX, out float flowY)
	{
		float dx = unit.MoveGoalX - unit.GameX;
		float dy = unit.MoveGoalY - unit.GameY;
		Normalize(dx, dy, out flowX, out flowY);
	}

	private static void SampleSeparation(IUnitHandle unit, IReadOnlyList<IUnitHandle> neighbors, out float sepX, out float sepY)
	{
		sepX = 0f;
		sepY = 0f;
		foreach (IUnitHandle other in neighbors)
		{
			if (other == unit || !other.IsAlive)
				continue;
			float dx = unit.GameX - other.GameX;
			float dy = unit.GameY - other.GameY;
			float dist = MathF.Sqrt(dx * dx + dy * dy);
			float minDist = unit.CollisionRadius + other.CollisionRadius + SeparationClearance;
			if (dist >= minDist || dist <= Epsilon)
				continue;
			float strength = (minDist - dist) / minDist;
			sepX += dx / dist * strength;
			sepY += dy / dist * strength;
		}
	}

	private static void ApplyWallSlide(IUnitHandle unit, NavGrid grid, ref float vx, ref float vy, float dt)
	{
		if (grid.IsBlockedAtWorld(unit.GameX + vx * dt, unit.GameY))
			vx = 0f;
		if (grid.IsBlockedAtWorld(unit.GameX, unit.GameY + vy * dt))
			vy = 0f;
	}

	private static void Normalize(float x, float y, out float nx, out float ny)
	{
		float length = MathF.Sqrt(x * x + y * y);
		if (length <= Epsilon)
		{
			nx = 0f;
			ny = 0f;
			return;
		}
		nx = x / length;
		ny = y / length;
	}
}
