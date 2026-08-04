using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class UnitBehaviorTests
{
	[Fact]
	public void UpdateTargeting_EnemyInVision_EntersFighting()
	{
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		Unit self = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		target.SetPosition(300, 0);
		self.SetPosition(0, 0);

		self.UpdateTargeting(target);

		Assert.Equal(UnitState.Fighting, self.State);
		Assert.Same(target, self.TargetEnemy);
	}

	[Fact]
	public void UpdateTargeting_RetaliationTargetOutsideVision_EntersRetaliating()
	{
		Unit attacker = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		Unit self = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		self.SetEnemyTarget(attacker);

		self.UpdateTargeting(null);

		Assert.Equal(UnitState.Retaliating, self.State);
	}

	[Fact]
	public void UpdateTargeting_NoTargets_EntersMarching()
	{
		Unit self = UnitTestFactory.CreateUnit(AttackType.Melee, true);

		self.UpdateTargeting(null);

		Assert.Equal(UnitState.Marching, self.State);
		Assert.Null(self.TargetEnemy);
	}

	[Fact]
	public void UpdateBehavior_FightingInAttackRange_DealsDamage()
	{
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		Unit self = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		target.SetPosition(120, 0);
		self.SetPosition(0, 0);
		self.SetEnemyTarget(target);
		self.UpdateTargeting(target);

		self.UpdateBehavior(2f, 0, 0);

		Assert.True(target.Health < target.MaxHealth);
	}

	[Fact]
	public void UpdateBehavior_FightingInAttackRange_StandsStill()
	{
		RecordingNavPort navPort = new();
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		Unit self = UnitTestFactory.CreateUnit(AttackType.Melee, true, navPort);
		target.SetPosition(120, 0);
		self.SetPosition(0, 0);
		self.SetEnemyTarget(target);
		self.UpdateTargeting(target);

		self.UpdateBehavior(2f, 0, 0);

		Assert.Equal(self.GameX, navPort.LastTargetX);
		Assert.Equal(self.GameY, navPort.LastTargetY);
	}

	[Fact]
	public void UpdateBehavior_Marching_SetsMarchTarget()
	{
		RecordingNavPort navPort = new();
		Unit self = UnitTestFactory.CreateUnit(AttackType.Melee, true, navPort);
		self.UpdateTargeting(null);

		self.UpdateBehavior(0.016f, 500, 800);

		Assert.Equal(500f, navPort.LastTargetX);
		Assert.Equal(800f, navPort.LastTargetY);
	}

	[Fact]
	public void UpdateBehavior_StealthedAssassinAttacks_BreaksStealth()
	{
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		Unit self = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		self.Skills.Add(UnitTestFactory.CreateStealthSkill(180f, 0.85f));
		self.SetStealth(true);
		target.SetPosition(120, 0);
		self.SetPosition(0, 0);
		self.SetEnemyTarget(target);
		self.UpdateTargeting(target);

		self.UpdateBehavior(2f, 0, 0);

		Assert.False(self.IsStealthed);
	}

	[Fact]
	public void TakeDamage_AoeWhileStealthed_BreaksStealth()
	{
		Unit attacker = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		Unit self = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		self.Skills.Add(UnitTestFactory.CreateStealthSkill(180f, 0.85f));
		self.SetStealth(true);
		self.SetPosition(0, 0);

		self.TakeDamage(10, attacker, 100, 0, true);

		Assert.False(self.IsStealthed);
	}

	[Fact]
	public void TakeDamage_TargetedWhileStealthed_KeepsStealth()
	{
		Unit attacker = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		Unit self = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		self.Skills.Add(UnitTestFactory.CreateStealthSkill(180f, 0.85f));
		self.SetStealth(true);
		self.SetPosition(0, 0);

		self.TakeDamage(10, attacker, 100, 0);

		Assert.True(self.IsStealthed);
	}

	[Fact]
	public void GetCurrentSpeed_WhileStealthed_AppliesSpeedMultiplier()
	{
		Unit self = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		self.Skills.Add(UnitTestFactory.CreateStealthSkill(180f, 0.85f));
		self.SetStealth(true);

		Assert.Equal(self.Speed * 0.85f, self.GetCurrentSpeed());
	}

	[Fact]
	public void GetCurrentSpeed_NotStealthed_UsesBaseSpeed()
	{
		Unit self = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		self.Skills.Add(UnitTestFactory.CreateStealthSkill(180f, 0.85f));

		Assert.Equal(self.Speed, self.GetCurrentSpeed());
	}

	[Fact]
	public void UpdateBehavior_AttackBreaksStealth_StartsStealthCooldown()
	{
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		Unit self = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		self.Skills.Add(UnitTestFactory.CreateStealthSkill(180f, 0.85f, 3f));
		self.SetStealth(true);
		target.SetPosition(120, 0);
		self.SetPosition(0, 0);
		self.SetEnemyTarget(target);
		self.UpdateTargeting(target);

		self.UpdateBehavior(2f, 0, 0);

		Assert.Equal(3f, self.GetSkillCooldownRemaining("stealth"));
	}

	[Fact]
	public void TickSkillCooldowns_ReducesRemaining()
	{
		Unit self = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		self.Skills.Add(UnitTestFactory.CreateStealthSkill(180f, 0.85f, 3f));
		self.SetStealth(true);
		self.SetPosition(0, 0);
		Unit attacker = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		self.TakeDamage(10, attacker, 100, 0, true);

		self.UpdateBehavior(0.5f, 0, 0);

		Assert.Equal(2.5f, self.GetSkillCooldownRemaining("stealth"), 2);
	}

	[Fact]
	public void GetSkillCooldownRemaining_NoCooldown_ReturnsZero()
	{
		Unit self = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		self.Skills.Add(UnitTestFactory.CreateStealthSkill(180f, 0.85f));

		Assert.Equal(0f, self.GetSkillCooldownRemaining("stealth"));
	}
}
