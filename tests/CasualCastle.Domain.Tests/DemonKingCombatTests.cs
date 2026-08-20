using System.Collections.Generic;
using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class DemonKingCombatTests
{
	[Fact]
	public void TakeDamage_DemonKing_IgnoresDamageAndDrainsComfort()
	{
		Unit king = CreateKing();
		ComfortService comfort = new();
		king.Resources = new FactionResources(null, null, comfort);
		Unit attacker = UnitTestFactory.CreateUnit(AttackType.Melee, false);

		king.TakeDamage(60, attacker, 0f, 0f);

		Assert.True(king.IsAlive);
		Assert.Equal(1500, king.Health);
		Assert.Equal(DemonKingRules.MaxComfort - DemonKingRules.HitComfortCost, comfort.Value);
	}

	[Fact]
	public void TakeDamage_RegularUnit_StillTakesDamage()
	{
		Unit attacker = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, true);

		target.TakeDamage(30, attacker, 0f, 0f);

		Assert.Equal(70, target.Health);
	}

	[Fact]
	public void TryAttack_DemonKing_DrainsComfortOnAttack()
	{
		Unit king = CreateKing();
		ComfortService comfort = new();
		king.Resources = new FactionResources(null, null, comfort);
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		king.SetPosition(0f, 0f);
		target.SetPosition(50f, 0f);
		king.SetEnemyTarget(target);
		king.UpdateTargeting(target);

		king.UpdateBehavior(1f, 0f, 0f);

		Assert.True(comfort.Value < DemonKingRules.MaxComfort);
	}

	[Fact]
	public void UpdateTargeting_DefenderEnemyWithinPatrolRadius_Fights()
	{
		Unit king = CreateKing();
		king.SetDefender(0f, 0f, 200f);
		Unit enemy = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		enemy.SetPosition(100f, 0f);

		king.UpdateTargeting(enemy);

		Assert.Equal(UnitState.Fighting, king.State);
		Assert.Same(enemy, king.TargetEnemy);
	}

	[Fact]
	public void UpdateTargeting_DefenderEnemyBeyondPatrolRadius_Waits()
	{
		Unit king = CreateKing();
		king.SetDefender(0f, 0f, 200f);
		Unit enemy = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		enemy.SetPosition(500f, 0f);

		king.UpdateTargeting(enemy);

		Assert.Equal(UnitState.Waiting, king.State);
		Assert.Null(king.TargetEnemy);
	}

	[Fact]
	public void UpdateTargeting_DefenderOutOfPatrolRadius_MarchesBack()
	{
		Unit king = CreateKing();
		king.SetDefender(0f, 0f, 200f);
		king.SetPosition(400f, 0f);

		king.UpdateTargeting(null);

		Assert.Equal(UnitState.Marching, king.State);
	}

	[Fact]
	public void AuraService_DemonRace_AppliesSuccubusAura()
	{
		SkillDef aura = new()
		{
			Id = "succubus_aura",
			DisplayName = "魅惑",
			Type = SkillType.Aura,
			ConfigJson = "{\"target\":\"nearby_allies\",\"range\":150,\"race\":\"Demon\",\"bonus_type\":\"attack_damage\",\"value\":0.15}"
		};
		Unit king = CreateKing();
		king.SetPosition(0f, 0f);
		Unit succubus = UnitTestFactory.CreateUnit(AttackType.Ranged, true);
		succubus.SetPosition(30f, 0f);
		succubus.Race = DemonKingRules.DemonRace;
		succubus.Skills.Add(aura);

		AuraService service = new();
		AuraEffects effects = service.CalculateEffects(king, new List<IUnitHandle> { king, succubus });

		Assert.Equal(1.15f, effects.AttackDamageMultiplier, 2);
	}

	[Fact]
	public void AuraService_HumanRace_IgnoresDemonAura()
	{
		SkillDef aura = new()
		{
			Id = "succubus_aura",
			DisplayName = "魅惑",
			Type = SkillType.Aura,
			ConfigJson = "{\"target\":\"nearby_allies\",\"range\":150,\"race\":\"Demon\",\"bonus_type\":\"attack_damage\",\"value\":0.15}"
		};
		Unit human = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		Unit succubus = UnitTestFactory.CreateUnit(AttackType.Ranged, true);
		succubus.Race = DemonKingRules.DemonRace;
		succubus.Skills.Add(aura);

		AuraService service = new();
		AuraEffects effects = service.CalculateEffects(human, new List<IUnitHandle> { human, succubus });

		Assert.Equal(1f, effects.AttackDamageMultiplier, 2);
	}

	private static Unit CreateKing()
	{
		UnitStats stats = new()
		{
			TypeId = "DemonKing",
			Size = UnitSize.Huge,
			AttackType = AttackType.Melee,
			DamageType = DamageType.Chaos,
			ArmorType = ArmorType.Plate,
			Health = 1500,
			Damage = 45,
			Speed = 85,
			AttackRange = 60,
			AttackCooldown = 1f,
			VisionRange = 260,
			Race = DemonKingRules.DemonKingRace,
		};
		Unit king = new();
		king.Initialize(stats, true);
		king.SetDamageMatrix(new DamageMatrix());
		return king;
	}
}