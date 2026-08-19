// 单位音效表现：攻击/受击音效事件映射
using CasualCastle.Domain.Battle;

public sealed class UnitSfx
{
	public static string GetAttackSfxEvent(AttackType attackType, DamageType damageType)
	{
		if (attackType == AttackType.Melee)
		{
			switch (damageType)
			{
				case DamageType.Slash:
					return SfxEvents.MeleeSlash;
				case DamageType.Pierce:
					return SfxEvents.MeleePierce;
				default:
					return SfxEvents.MeleeAttack;
			}
		}

		switch (damageType)
		{
			case DamageType.Pierce:
				return SfxEvents.LaunchArrow;
			case DamageType.Magic:
				return SfxEvents.LaunchMagic;
			default:
				return SfxEvents.ProjectileLaunch;
		}
	}

	public static string GetHitSfxEvent(IUnitHandle attacker)
	{
		if (attacker == null)
			return SfxEvents.Hit;

		switch (attacker.DamageType)
		{
			case DamageType.Slash:
				return SfxEvents.HitSlash;
			case DamageType.Pierce:
				return SfxEvents.HitPierce;
			case DamageType.Magic:
				return SfxEvents.HitMagic;
			default:
				return SfxEvents.Hit;
		}
	}
}
