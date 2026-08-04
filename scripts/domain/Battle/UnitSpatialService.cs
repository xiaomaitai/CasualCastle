using System;
using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public class UnitSpatialService : ICombatUseCase
{
	private const float PushForce = 500f;
	private const float UnitClearance = 4f;
	private const float BuildingClearance = 4f;

	public void PushUnits(IReadOnlyList<IUnitHandle> allUnits, IReadOnlyList<IBuildingRef> buildings, float dt)
	{
		PushUnitsApart(allUnits, dt);
		PushOutOfBuildings(allUnits, buildings);
	}

	private static void PushUnitsApart(IReadOnlyList<IUnitHandle> allUnits, float dt)
	{
		for (int i = 0; i < allUnits.Count; i++)
		{
			IUnitHandle a = allUnits[i];
			if (!a.IsAlive || IsEngaged(a))
				continue;

			for (int j = i + 1; j < allUnits.Count; j++)
			{
				IUnitHandle b = allUnits[j];
				if (!b.IsAlive || IsEngaged(b))
					continue;

				float dx = a.GameX - b.GameX;
				float dy = a.GameY - b.GameY;
				float dist = MathF.Sqrt(dx * dx + dy * dy);
				float minDist = a.CollisionRadius + b.CollisionRadius + UnitClearance;
				if (dist < minDist && dist > 0.001f)
				{
					float pushAmount = (minDist - dist) * PushForce * dt / minDist;
					a.ApplyPush(dx * pushAmount, dy * pushAmount);
					b.ApplyPush(-dx * pushAmount, -dy * pushAmount);
				}
			}
		}
	}

	private static void PushOutOfBuildings(IReadOnlyList<IUnitHandle> allUnits, IReadOnlyList<IBuildingRef> buildings)
	{
		foreach (IUnitHandle unit in allUnits)
		{
			if (!unit.IsAlive || IsEngaged(unit))
				continue;
			foreach (IBuildingRef building in buildings)
			{
				if (building.IsDestroyed || building.IsCore || !building.IsEnemyOf(unit))
					continue;
				PushOutOfBuilding(unit, building);
			}
		}
	}

	private static bool IsEngaged(IUnitHandle unit)
	{
		return unit.State == UnitState.Fighting
			|| unit.State == UnitState.Retaliating
			|| unit.State == UnitState.Sieging
			|| unit.State == UnitState.Healing;
	}

	private static void PushOutOfBuilding(IUnitHandle unit, IBuildingRef building)
	{
		float closestX = Math.Clamp(unit.GameX, building.MinX, building.MaxX);
		float closestY = Math.Clamp(unit.GameY, building.MinY, building.MaxY);
		float dx = unit.GameX - closestX;
		float dy = unit.GameY - closestY;
		float distanceSq = dx * dx + dy * dy;
		float radiusSq = unit.CollisionRadius * unit.CollisionRadius;
		if (distanceSq >= radiusSq)
			return;
		if (distanceSq > 0.001f)
		{
			float distance = MathF.Sqrt(distanceSq);
			float push = unit.CollisionRadius - distance + BuildingClearance;
			unit.ApplyPush(dx / distance * push, dy / distance * push);
		}
		else
		{
			PushFromBuildingInterior(unit, building);
		}
	}

	private static void PushFromBuildingInterior(IUnitHandle unit, IBuildingRef building)
	{
		float distanceToLeft = unit.GameX - building.MinX;
		float distanceToRight = building.MaxX - unit.GameX;
		float distanceToTop = unit.GameY - building.MinY;
		float distanceToBottom = building.MaxY - unit.GameY;
		float minAxisX = Math.Min(distanceToLeft, distanceToRight);
		float minAxisY = Math.Min(distanceToTop, distanceToBottom);
		float clearance = unit.CollisionRadius + BuildingClearance;
		if (minAxisX < minAxisY)
		{
			if (distanceToLeft < distanceToRight)
				unit.ApplyPush(building.MinX - clearance - unit.GameX, 0f);
			else
				unit.ApplyPush(building.MaxX + clearance - unit.GameX, 0f);
		}
		else
		{
			if (distanceToTop < distanceToBottom)
				unit.ApplyPush(0f, building.MinY - clearance - unit.GameY);
			else
				unit.ApplyPush(0f, building.MaxY + clearance - unit.GameY);
		}
	}
}
