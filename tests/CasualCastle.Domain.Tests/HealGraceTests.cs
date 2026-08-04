using System.Collections.Generic;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Shared;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class HealRulesTests
{
	[Fact]
	public void ApplyHeal_HealsByAmount()
	{
		Assert.Equal(80, HealRules.ApplyHeal(50, 100, 30));
	}

	[Fact]
	public void ApplyHeal_CapsAtMaxHealth()
	{
		Assert.Equal(100, HealRules.ApplyHeal(90, 100, 30));
	}

	[Fact]
	public void ApplyHeal_DeadUnit_ReturnsCurrentHealth()
	{
		Assert.Equal(0, HealRules.ApplyHeal(0, 100, 30));
	}

	[Fact]
	public void ApplyHeal_NonPositiveAmount_ReturnsCurrentHealth()
	{
		Assert.Equal(50, HealRules.ApplyHeal(50, 100, 0));
	}
}

public class UnitHealTests
{
	[Fact]
	public void Heal_RestoresHealthAndReturnsActual()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		unit.TakeDamage(40, null, 0f, 0f);

		int healed = unit.Heal(30);

		Assert.Equal(30, healed);
		Assert.Equal(90, unit.Health);
	}

	[Fact]
	public void Heal_CapsAtMaxHealth()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		unit.TakeDamage(20, null, 0f, 0f);

		int healed = unit.Heal(50);

		Assert.Equal(20, healed);
		Assert.Equal(100, unit.Health);
	}

	[Fact]
	public void Heal_FullHealth_ReturnsZero()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);

		Assert.Equal(0, unit.Heal(30));
	}

	[Fact]
	public void Heal_DeadUnit_ReturnsZero()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		unit.TakeDamage(200, null, 0f, 0f);

		Assert.Equal(0, unit.Heal(30));
	}
}

public class GraceShieldTests
{
	[Fact]
	public void Heal_WithGraceShield_AddsShield()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		unit.Skills.Add(CreateShieldSkill());
		unit.TakeDamage(50, null, 0f, 0f);

		unit.Heal(30);

		Assert.Equal(45, unit.ShieldHealth);
	}

	[Fact]
	public void Heal_WithGraceShield_CapsAtMaxHealth()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		unit.Skills.Add(CreateShieldSkill());
		unit.TakeDamage(99, null, 0f, 0f);

		unit.Heal(99);

		Assert.Equal(100, unit.ShieldHealth);
	}

	[Fact]
	public void Heal_WithGraceShield_ShieldStacks()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		unit.Skills.Add(CreateShieldSkill());
		unit.TakeDamage(90, null, 0f, 0f);
		unit.Heal(30);
		unit.Heal(30);

		Assert.Equal(90, unit.ShieldHealth);
	}

	[Fact]
	public void TakeDamage_ShieldAbsorbsBeforeHealth()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		unit.Skills.Add(CreateShieldSkill());
		unit.TakeDamage(50, null, 0f, 0f);
		unit.Heal(30);

		unit.TakeDamage(30, null, 0f, 0f);

		Assert.Equal(15, unit.ShieldHealth);
		Assert.Equal(80, unit.Health);
	}

	[Fact]
	public void TakeDamage_ShieldOverflowDamagesHealth()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		unit.Skills.Add(CreateShieldSkill());
		unit.TakeDamage(50, null, 0f, 0f);
		unit.Heal(30);

		unit.TakeDamage(60, null, 0f, 0f);

		Assert.Equal(0, unit.ShieldHealth);
		Assert.Equal(65, unit.Health);
	}

	private static SkillDef CreateShieldSkill()
	{
		return new SkillDef
		{
			Id = "grace_shield",
			DisplayName = "Grace Shield",
			Type = SkillType.OnHeal,
			ConfigJson = "{\"effects\":[{\"type\":\"shield\"}]}"
		};
	}
}

public class GraceCooldownReductionTests
{
	[Fact]
	public void Heal_WithGraceCooldown_ReducesRemainingCooldown()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		unit.Skills.Add(UnitTestFactory.CreateStealthSkill(180f, 0.85f, 10f));
		unit.Skills.Add(CreateCooldownSkill());
		unit.StartSkillCooldown("stealth");
		unit.TakeDamage(30, null, 0f, 0f);

		unit.Heal(30);

		Assert.Equal(9f, unit.GetSkillCooldownRemaining("stealth"), 3);
	}

	[Fact]
	public void Heal_WithGraceCooldown_ExpiresCooldownBelowZero()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		unit.Skills.Add(UnitTestFactory.CreateStealthSkill(180f, 0.85f, 10f));
		unit.Skills.Add(CreateCooldownSkill());
		unit.StartSkillCooldown("stealth");
		unit.TickSkillCooldowns(9.2f);
		unit.TakeDamage(30, null, 0f, 0f);

		unit.Heal(30);

		Assert.Equal(0f, unit.GetSkillCooldownRemaining("stealth"));
	}

	private static SkillDef CreateCooldownSkill()
	{
		return new SkillDef
		{
			Id = "grace_cooldown",
			DisplayName = "Grace Cooldown",
			Type = SkillType.OnHeal,
			ConfigJson = "{\"effects\":[{\"type\":\"cooldown_reduction\",\"value\":1.0}]}"
		};
	}
}

public class GraceBurnTests
{
	[Fact]
	public void Heal_WithGraceBurn_DamagesNearbyEnemies()
	{
		Unit healer = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		healer.Skills.Add(CreateBurnSkill(15f, 120f));
		Unit enemy = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		enemy.SetPosition(50f, 0f);
		healer.Tactical = CreateTactical(healer, enemy);
		healer.TakeDamage(30, null, 0f, 0f);

		healer.Heal(30);

		Assert.Equal(85, enemy.Health);
	}

	[Fact]
	public void Heal_WithGraceBurn_IgnoresArmor()
	{
		Unit healer = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		healer.Skills.Add(CreateBurnSkill(20f, 120f));
		UnitStats stats = new()
		{
			TypeId = "armored",
			Size = UnitSize.Medium,
			AttackType = AttackType.Melee,
			DamageType = DamageType.Slash,
			ArmorType = ArmorType.Plate,
			ArmorValue = 1000,
			Health = 100,
			Damage = 10,
			Speed = 10f,
			AttackRange = 30f,
			AttackCooldown = 1f,
			VisionRange = 300f
		};
		Unit enemy = new(new RecordingNavPort());
		enemy.Initialize(stats, false);
		enemy.SetDamageMatrix(new DamageMatrix());
		enemy.SetPosition(50f, 0f);
		healer.Tactical = CreateTactical(healer, enemy);
		healer.TakeDamage(30, null, 0f, 0f);

		healer.Heal(30);

		Assert.Equal(80, enemy.Health);
	}

	[Fact]
	public void Heal_WithGraceBurn_IgnoresFarEnemies()
	{
		Unit healer = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		healer.Skills.Add(CreateBurnSkill(15f, 120f));
		Unit nearEnemy = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		nearEnemy.SetPosition(50f, 0f);
		Unit farEnemy = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		farEnemy.SetPosition(300f, 0f);
		healer.Tactical = CreateTactical(healer, nearEnemy, farEnemy);
		healer.TakeDamage(30, null, 0f, 0f);

		healer.Heal(30);

		Assert.Equal(85, nearEnemy.Health);
		Assert.Equal(100, farEnemy.Health);
	}

	private static SkillDef CreateBurnSkill(float damage, float range)
	{
		string damageText = damage.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture);
		string rangeText = range.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture);
		return new SkillDef
		{
			Id = "grace_burn",
			DisplayName = "Grace Burn",
			Type = SkillType.OnHeal,
			ConfigJson = "{\"effects\":[{\"type\":\"burn\",\"value\":" + damageText + ",\"range\":" + rangeText + "}]}"
		};
	}

	private static ITacticalQueries CreateTactical(IUnitHandle center, params IUnitHandle[] enemies)
	{
		FakeTacticalQueries tactical = new();
		tactical.Enemies.AddRange(enemies);
		return tactical;
	}
}

internal sealed class FakeTacticalQueries : ITacticalQueries
{
	public List<IUnitHandle> Enemies { get; } = new();

	public (IUnitHandle nearest, float edgeDist) FindNearestEnemy(IUnitHandle unit)
	{
		return (null, float.MaxValue);
	}

	public void PropagateRetaliation(IUnitHandle center, IUnitHandle attacker)
	{
	}

	public IBuildingTarget FindOverlappingBuilding(IUnitHandle unit)
	{
		return null;
	}

	public bool HasEnemyOnBuilding(IBuildingRef building)
	{
		return false;
	}

	public IReadOnlyList<IUnitHandle> FindEnemiesNear(IUnitHandle center, float radius)
	{
		List<IUnitHandle> found = new();
		float radiusSq = radius * radius;
		foreach (IUnitHandle enemy in Enemies)
		{
			float dx = enemy.GameX - center.GameX;
			float dy = enemy.GameY - center.GameY;
			if (dx * dx + dy * dy <= radiusSq)
				found.Add(enemy);
		}
		return found;
	}

	public IUnitHandle FindNearestInjuredAlly(IUnitHandle unit, float range)
	{
		return null;
	}
}
