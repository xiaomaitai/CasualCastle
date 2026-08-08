// 潜行服务：隐身状态与检测更新
namespace CasualCastle.Domain.Battle;

public class StealthService
{
	public void UpdateStealth(IUnitHandle unit, IFieldUnitRepository repo)
	{
		if (!unit.IsAlive)
			return;

		SpecialConfig stealth = unit.Skills.GetSpecial(SpecialBehavior.Stealth);
		if (stealth == null)
		{
			unit.SetStealth(false);
			return;
		}

		bool hasDetector = EnemyHasDetector(unit, repo);
		bool shouldStealth = unit.IsStealthed
			? !hasDetector
			: !hasDetector && !IsOnCooldown(unit, stealth) && IsFarFromEnemies(unit, stealth.Param, repo);

		if (unit.IsStealthed != shouldStealth)
		{
			unit.SetStealth(shouldStealth);
			if (shouldStealth)
				ClearEnemyTargetsOf(repo, unit);
		}
	}

	private static bool EnemyHasDetector(IUnitHandle unit, IFieldUnitRepository repo)
	{
		foreach (IUnitHandle enemy in repo.AllUnits)
		{
			if (!enemy.IsAlive || enemy.IsPlayerUnit == unit.IsPlayerUnit)
				continue;
			if (enemy.CanDetectStealth)
				return true;
		}
		return false;
	}

	private static bool IsFarFromEnemies(IUnitHandle unit, float engageDistance, IFieldUnitRepository repo)
	{
		foreach (IUnitHandle enemy in repo.AllUnits)
		{
			if (!enemy.IsAlive || enemy.IsPlayerUnit == unit.IsPlayerUnit)
				continue;
			float dx = unit.GameX - enemy.GameX;
			float dy = unit.GameY - enemy.GameY;
			if (dx * dx + dy * dy <= engageDistance * engageDistance)
				return false;
		}
		return true;
	}

	private static bool IsOnCooldown(IUnitHandle unit, SpecialConfig stealth)
	{
		return unit.GetSkillCooldownRemaining(stealth.SkillId) > 0f;
	}

	private static void ClearEnemyTargetsOf(IFieldUnitRepository repo, IUnitHandle unit)
	{
		foreach (IUnitHandle enemy in repo.AllUnits)
		{
			if (enemy.IsPlayerUnit == unit.IsPlayerUnit)
				continue;
			if (enemy.TargetEnemy == unit)
				enemy.SetEnemyTarget(null);
		}
	}
}
