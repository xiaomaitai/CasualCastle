using System;
using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public class UnitSpatialService : ICombatUseCase
{
	private const float PushForce = 500f;

	public void PushUnits(IReadOnlyList<IUnitHandle> allUnits, float dt)
	{
		for (int i = 0; i < allUnits.Count; i++)
		{
			IUnitHandle a = allUnits[i];
			if (!a.IsAlive || a.State == UnitState.Sieging)
				continue;

			for (int j = i + 1; j < allUnits.Count; j++)
			{
				IUnitHandle b = allUnits[j];
				if (!b.IsAlive || b.State == UnitState.Sieging)
					continue;

				float dx = a.GameX - b.GameX;
				float dy = a.GameY - b.GameY;
				float dist = MathF.Sqrt(dx * dx + dy * dy);
				float minDist = a.CollisionRadius + b.CollisionRadius + 4f;
				if (dist < minDist && dist > 0.001f)
				{
					float pushAmount = (minDist - dist) * PushForce * dt / minDist;
					a.ApplyPush(dx * pushAmount, dy * pushAmount);
					b.ApplyPush(-dx * pushAmount, -dy * pushAmount);
				}
			}
		}
	}
}
