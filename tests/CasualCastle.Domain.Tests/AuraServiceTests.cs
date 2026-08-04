using System.Collections.Generic;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Shared;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class AuraServiceTests
{
	private static SkillDef CreateAuraSkill(float range)
	{
		string rangeText = range.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture);
		return new SkillDef
		{
			Id = "orc_rage_aura",
			DisplayName = "Fury Aura",
			Type = SkillType.Aura,
			ConfigJson = "{\"target\":\"nearby_allies\",\"range\":" + rangeText + "}"
		};
	}

	private static Unit CreateEmitter(string race, bool isPlayerUnit)
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, isPlayerUnit);
		unit.Race = race;
		unit.Skills.Add(CreateAuraSkill(150f));
		return unit;
	}

	[Fact]
	public void CalculateAttackMultiplier_SameRaceEmitters_NotStacked()
	{
		Unit receiver = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		Unit emitterA = CreateEmitter("Orc", true);
		Unit emitterB = CreateEmitter("Orc", true);
		emitterA.SetPosition(30, 0);
		emitterB.SetPosition(-30, 0);
		List<IUnitHandle> all = new() { receiver, emitterA, emitterB };
		AuraService service = new();

		float mult = service.CalculateAttackMultiplier(receiver, all);

		Assert.Equal(GameRules.AuraAttackBonusMultiplier, mult, 4);
	}

	[Fact]
	public void CalculateAttackMultiplier_DifferentRaces_Stack()
	{
		Unit receiver = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		Unit orcEmitter = CreateEmitter("Orc", true);
		Unit goblinEmitter = CreateEmitter("Goblin", true);
		orcEmitter.SetPosition(30, 0);
		goblinEmitter.SetPosition(-30, 0);
		List<IUnitHandle> all = new() { receiver, orcEmitter, goblinEmitter };
		AuraService service = new();

		float mult = service.CalculateAttackMultiplier(receiver, all);

		Assert.Equal(GameRules.AuraAttackBonusMultiplier * GameRules.AuraAttackBonusMultiplier, mult, 4);
	}

	[Fact]
	public void CalculateAttackMultiplier_OutOfRange_ReturnsOne()
	{
		Unit receiver = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		Unit emitter = CreateEmitter("Orc", true);
		emitter.SetPosition(200, 0);
		List<IUnitHandle> all = new() { receiver, emitter };
		AuraService service = new();

		float mult = service.CalculateAttackMultiplier(receiver, all);

		Assert.Equal(1f, mult, 4);
	}

	[Fact]
	public void CalculateAttackMultiplier_EnemyEmitter_ReturnsOne()
	{
		Unit receiver = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		Unit emitter = CreateEmitter("Orc", false);
		emitter.SetPosition(30, 0);
		List<IUnitHandle> all = new() { receiver, emitter };
		AuraService service = new();

		float mult = service.CalculateAttackMultiplier(receiver, all);

		Assert.Equal(1f, mult, 4);
	}

	[Fact]
	public void CalculateAttackMultiplier_DeadEmitter_ReturnsOne()
	{
		Unit receiver = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		Unit emitter = CreateEmitter("Orc", true);
		emitter.IsAlive = false;
		emitter.SetPosition(30, 0);
		List<IUnitHandle> all = new() { receiver, emitter };
		AuraService service = new();

		float mult = service.CalculateAttackMultiplier(receiver, all);

		Assert.Equal(1f, mult, 4);
	}

	[Fact]
	public void CalculateAttackMultiplier_DefaultRange_UsesGameRuleRadius()
	{
		Unit receiver = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		Unit emitter = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		emitter.Race = "Orc";
		emitter.Skills.Add(CreateAuraSkill(0f));
		emitter.SetPosition(GameRules.AuraRangeRadius, 0);
		List<IUnitHandle> all = new() { receiver, emitter };
		AuraService service = new();

		float mult = service.CalculateAttackMultiplier(receiver, all);

		Assert.Equal(GameRules.AuraAttackBonusMultiplier, mult, 4);
	}

	[Fact]
	public void UnitAttack_AuraMultiplier_IncreasesDamage()
	{
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		Unit attacker = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		target.SetPosition(120, 0);
		attacker.SetPosition(0, 0);
		attacker.SetEnemyTarget(target);
		attacker.UpdateTargeting(target);
		attacker.GameCtx.AuraAttackMultiplier = 1.21f;

		attacker.UpdateBehavior(2f, 0, 0);

		Assert.Equal(100 - 24, target.Health);
	}
}
