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
}
