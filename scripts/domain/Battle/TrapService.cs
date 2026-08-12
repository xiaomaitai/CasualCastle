// 陷阱触发服务：对范围内敌方单位结算伤害/毒素/传送
using System;
using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public sealed class TrapService
{
	private readonly ITacticalQueries _tactical;
	private readonly DamageMatrix _damageMatrix;

	public TrapService(ITacticalQueries tactical, DamageMatrix damageMatrix)
	{
		_tactical = tactical;
		_damageMatrix = damageMatrix;
	}

	public int Trigger(IUnitHandle source, string trapTypeId, float centerX, float centerY, float teleportX, float teleportY)
	{
		TrapConfig config = TrapRules.GetConfig(trapTypeId);
		IReadOnlyList<IUnitHandle> enemies = _tactical.FindEnemiesNearPoint(source, centerX, centerY, config.Range);
		int hitCount = 0;
		foreach (IUnitHandle enemy in enemies)
		{
			if (!enemy.IsAlive)
				continue;
			if (config.Damage > 0)
			{
				int damage = CombatRules.CalculateDamage(config.Damage, DamageType.Pierce, enemy.ArmorType, enemy.EffectiveArmorValue, _damageMatrix);
				enemy.TakeDamage(damage, null, centerX, centerY);
			}
			if (config.PoisonPerSecond > 0f)
				enemy.ApplyPoison(config.PoisonPerSecond, config.PoisonDuration);
			if (config.IsTeleport && enemy is Unit unit)
				unit.TeleportTo(teleportX, teleportY);
			hitCount++;
		}
		return hitCount;
	}

	public void PullTowards(IUnitHandle enemy, float targetX, float targetY, float speed, float dt)
	{
		if (enemy is not Unit unit || speed <= 0f || dt <= 0f)
			return;
		float dx = targetX - unit.GameX;
		float dy = targetY - unit.GameY;
		float distance = MathF.Sqrt(dx * dx + dy * dy);
		if (distance <= 0f)
			return;
		float step = MathF.Min(distance, speed * dt);
		unit.TeleportTo(unit.GameX + dx / distance * step, unit.GameY + dy / distance * step);
	}
}