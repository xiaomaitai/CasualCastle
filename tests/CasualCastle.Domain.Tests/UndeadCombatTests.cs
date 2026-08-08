using System.Collections.Generic;
using CasualCastle.Domain.Battle;
using Moq;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class UndeadCombatTests
{
	private static SkillDef CreatePoisonSkill(float damagePerSecond, float duration)
	{
		string valueText = damagePerSecond.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture);
		string durationText = duration.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture);
		return new SkillDef
		{
			Id = "poison",
			DisplayName = "Poison",
			Type = SkillType.OnHit,
			ConfigJson = "{\"effects\":[{\"type\":\"poison\",\"value\":" + valueText + ",\"duration\":" + durationText + "}]}"
		};
	}

	private static SkillDef CreatePlagueBurstSkill()
	{
		return new SkillDef
		{
			Id = "plague_burst",
			DisplayName = "Plague Burst",
			Type = SkillType.Special,
			ConfigJson = "{\"behavior\":\"plague_burst\",\"param\":80,\"damage_mult\":20}"
		};
	}

	[Fact]
	public void Initialize_StatsWithNightCombat_SetsFlag()
	{
		UnitStats stats = new()
		{
			TypeId = "Skeleton",
			Size = UnitSize.Small,
			AttackType = AttackType.Melee,
			DamageType = DamageType.Slash,
			ArmorType = ArmorType.Leather,
			Health = 25,
			Damage = 8,
			Speed = 70,
			HasNightCombat = true,
		};
		Unit unit = new(new RecordingNavPort());

		unit.Initialize(stats, true);

		Assert.True(unit.HasNightCombat);
	}

	[Fact]
	public void PoisonHit_TicksDamageOverTime()
	{
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		Unit self = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		self.Skills.Add(CreatePoisonSkill(3f, 3f));
		target.SetPosition(120, 0);
		self.SetPosition(0, 0);
		self.SetEnemyTarget(target);
		self.UpdateTargeting(target);

		self.UpdateBehavior(2f, 0, 0);
		int healthAfterHit = target.Health;

		target.UpdateBehavior(1f, 0, 0);

		Assert.Equal(healthAfterHit - 3, target.Health);
	}

	[Fact]
	public void PoisonExpires_AfterDuration()
	{
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		Unit self = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		self.Skills.Add(CreatePoisonSkill(3f, 3f));
		target.SetPosition(120, 0);
		self.SetPosition(0, 0);
		self.SetEnemyTarget(target);
		self.UpdateTargeting(target);

		self.UpdateBehavior(2f, 0, 0);
		target.UpdateBehavior(3f, 0, 0);
		int healthAfterPoison = target.Health;

		target.UpdateBehavior(5f, 0, 0);

		Assert.Equal(healthAfterPoison, target.Health);
	}

	[Fact]
	public void PlagueBurst_OnDeath_DamagesNearbyEnemies()
	{
		Unit carrier = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		carrier.Skills.Add(CreatePlagueBurstSkill());
		Unit enemy = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		Mock<ITacticalQueries> tactical = new();
		tactical.Setup(t => t.FindEnemiesNear(carrier, 80f)).Returns(new List<IUnitHandle> { enemy });
		carrier.Tactical = tactical.Object;

		carrier.TakeDamage(1000, null, 0, 0);

		Assert.False(carrier.IsAlive);
		Assert.Equal(enemy.MaxHealth - 20, enemy.Health);
	}

	[Fact]
	public void HolyDamageVsUndead_DoublesThroughDispel()
	{
		Unit undead = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		undead.Race = "Undead";
		undead.ArmorValue = 0;
		Unit holy = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		holy.DamageType = DamageType.Holy;
		holy.Damage = 10;
		holy.ArmorValue = 0;
		undead.SetPosition(120, 0);
		holy.SetPosition(0, 0);
		holy.SetEnemyTarget(undead);
		holy.UpdateTargeting(undead);

		holy.UpdateBehavior(2f, 0, 0);

		Assert.Equal(undead.MaxHealth - 20, undead.Health);
	}
}