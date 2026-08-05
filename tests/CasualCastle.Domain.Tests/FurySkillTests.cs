using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class FurySkillTests
{
	private const string OrcFuryConfig = "{\"trigger\":\"on_damaged\",\"race\":\"Orc\",\"modifiers\":{\"attack_damage_mult\":1.3,\"move_speed_mult\":1.15}}";
	private const string OrcBerserkerFuryConfig = "{\"trigger\":\"on_damaged\",\"race\":\"Orc\",\"modifiers\":{\"attack_damage_mult\":1.15,\"attack_speed_mult\":1.3,\"move_speed_mult\":1.15}}";

	private static SkillDef CreateFurySkill(string json)
	{
		return new SkillDef
		{
			Id = "fury",
			DisplayName = "Fury",
			Type = SkillType.StatModifier,
			ConfigJson = json
		};
	}

	private static SkillSet CreateSkillSet(string json)
	{
		SkillSet skills = new();
		skills.Add(CreateFurySkill(json));
		return skills;
	}

	[Fact]
	public void OnDamaged_OrcFurious_AppliesDamageMultiplier()
	{
		SkillSet skills = CreateSkillSet(OrcFuryConfig);
		GameContext ctx = new() { Race = "Orc", IsFurious = true };

		Assert.Equal(1.3f, skills.GetStatMultiplier("attack_damage_mult", ctx), 4);
	}

	[Fact]
	public void OnDamaged_OrcFurious_AppliesMoveSpeedMultiplier()
	{
		SkillSet skills = CreateSkillSet(OrcFuryConfig);
		GameContext ctx = new() { Race = "Orc", IsFurious = true };

		Assert.Equal(1.15f, skills.GetStatMultiplier("move_speed_mult", ctx), 4);
	}

	[Fact]
	public void OnDamaged_NotFurious_ReturnsOne()
	{
		SkillSet skills = CreateSkillSet(OrcFuryConfig);
		GameContext ctx = new() { Race = "Orc", IsFurious = false };

		Assert.Equal(1f, skills.GetStatMultiplier("attack_damage_mult", ctx), 4);
	}

	[Fact]
	public void OnDamaged_NonOrcRace_ReturnsOne()
	{
		SkillSet skills = CreateSkillSet(OrcFuryConfig);
		GameContext ctx = new() { Race = "Human", IsFurious = true };

		Assert.Equal(1f, skills.GetStatMultiplier("attack_damage_mult", ctx), 4);
	}

	[Fact]
	public void OnDamaged_NoRaceRequirement_AppliesToAnyRace()
	{
		SkillSet skills = CreateSkillSet("{\"trigger\":\"on_damaged\",\"modifiers\":{\"attack_damage_mult\":1.3}}");
		GameContext ctx = new() { Race = "Human", IsFurious = true };

		Assert.Equal(1.3f, skills.GetStatMultiplier("attack_damage_mult", ctx), 4);
	}

	[Fact]
	public void BerserkerFury_AppliesDamageAttackSpeedAndMoveSpeedMultipliers()
	{
		SkillSet skills = CreateSkillSet(OrcBerserkerFuryConfig);
		GameContext ctx = new() { Race = "Orc", IsFurious = true };

		Assert.Equal(1.15f, skills.GetStatMultiplier("attack_damage_mult", ctx), 4);
		Assert.Equal(1.3f, skills.GetStatMultiplier("attack_speed_mult", ctx), 4);
		Assert.Equal(1.15f, skills.GetStatMultiplier("move_speed_mult", ctx), 4);
	}

	[Fact]
	public void TakeDamage_WithFurySkill_EntersFury()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		unit.Race = "Orc";
		unit.Skills.Add(CreateFurySkill(OrcFuryConfig));

		unit.TakeDamage(10, null, 0, 0);

		Assert.True(unit.IsFurious);
	}

	[Fact]
	public void TakeDamage_WithoutFurySkill_StaysCalm()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		unit.Race = "Orc";

		unit.TakeDamage(10, null, 0, 0);

		Assert.False(unit.IsFurious);
	}

	[Fact]
	public void TakeDamage_ShieldAbsorbsAllDamage_DoesNotTriggerFury()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		unit.Race = "Orc";
		unit.Skills.Add(CreateFurySkill(OrcFuryConfig));
		unit.AddShield(100);

		unit.TakeDamage(10, null, 0, 0);

		Assert.False(unit.IsFurious);
	}

	[Fact]
	public void Fury_IsPermanent_DoesNotDecayOverTime()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		unit.Race = "Orc";
		unit.Skills.Add(CreateFurySkill(OrcFuryConfig));
		unit.TakeDamage(10, null, 0, 0);

		unit.UpdateBehavior(100f, 0, 0);

		Assert.True(unit.IsFurious);
	}

	[Fact]
	public void FuryActive_Orc_AttackDamageIsIncreased()
	{
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		Unit attacker = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		attacker.Race = "Orc";
		attacker.Skills.Add(CreateFurySkill(OrcFuryConfig));
		attacker.GameCtx = new GameContext { Race = "Orc", IsFurious = true };
		target.SetPosition(120, 0);
		attacker.SetPosition(0, 0);
		attacker.SetEnemyTarget(target);
		attacker.UpdateTargeting(target);

		attacker.UpdateBehavior(2f, 0, 0);

		Assert.Equal(100 - 26, target.Health);
	}

	[Fact]
	public void FuryActive_NonOrc_AttackDamageNotIncreased()
	{
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		Unit attacker = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		attacker.Race = "Human";
		attacker.Skills.Add(CreateFurySkill(OrcFuryConfig));
		attacker.GameCtx = new GameContext { Race = "Human", IsFurious = true };
		target.SetPosition(120, 0);
		attacker.SetPosition(0, 0);
		attacker.SetEnemyTarget(target);
		attacker.UpdateTargeting(target);

		attacker.UpdateBehavior(2f, 0, 0);

		Assert.Equal(100 - 20, target.Health);
	}

	[Fact]
	public void FuryActive_Orc_MoveSpeedIsIncreased()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		unit.Race = "Orc";
		unit.Skills.Add(CreateFurySkill(OrcFuryConfig));
		unit.GameCtx = new GameContext { Race = "Orc", IsFurious = true };

		Assert.Equal(unit.Speed * 1.15f, unit.GetCurrentSpeed(), 4);
	}

	[Fact]
	public void NotFurious_MoveSpeedIsBaseSpeed()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		unit.Race = "Orc";
		unit.Skills.Add(CreateFurySkill(OrcFuryConfig));
		unit.GameCtx = new GameContext { Race = "Orc", IsFurious = false };

		Assert.Equal(unit.Speed, unit.GetCurrentSpeed(), 4);
	}
}
