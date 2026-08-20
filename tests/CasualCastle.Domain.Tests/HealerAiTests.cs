using System.Collections.Generic;
using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class FindNearestInjuredAllyTests
{
	[Fact]
	public void FindNearestInjuredAlly_ReturnsNearestInjured()
	{
		FakeUnitRepository repo = new();
		Unit self = CreateAlly(repo, 0f);
		Unit healthy = CreateAlly(repo, 10f);
		Unit injured = CreateAlly(repo, 30f);
		injured.TakeDamage(20, null, 0f, 0f);
		Unit farInjured = CreateAlly(repo, 200f);
		farInjured.TakeDamage(20, null, 0f, 0f);
		TacticalQueryService tactical = new(repo);

		IUnitHandle found = tactical.FindNearestInjuredAlly(self, 100f);

		Assert.Same(injured, found);
		Assert.Equal(100, healthy.Health);
	}

	[Fact]
	public void FindNearestInjuredAlly_IgnoresEnemies()
	{
		FakeUnitRepository repo = new();
		Unit self = CreateAlly(repo, 0f);
		Unit enemy = CreateEnemy(repo, 20f);
		enemy.TakeDamage(20, null, 0f, 0f);
		TacticalQueryService tactical = new(repo);

		IUnitHandle found = tactical.FindNearestInjuredAlly(self, 100f);

		Assert.Null(found);
	}

	[Fact]
	public void FindNearestInjuredAlly_OutOfRange_ReturnsNull()
	{
		FakeUnitRepository repo = new();
		Unit self = CreateAlly(repo, 0f);
		Unit injured = CreateAlly(repo, 200f);
		injured.TakeDamage(20, null, 0f, 0f);
		TacticalQueryService tactical = new(repo);

		IUnitHandle found = tactical.FindNearestInjuredAlly(self, 100f);

		Assert.Null(found);
	}

	private static Unit CreateAlly(FakeUnitRepository repo, float offsetX)
	{
		Unit unit = CreateUnit(true);
		unit.SetPosition(offsetX, 0f);
		repo.Register(unit);
		return unit;
	}

	private static Unit CreateEnemy(FakeUnitRepository repo, float offsetX)
	{
		Unit unit = CreateUnit(false);
		unit.SetPosition(offsetX, 0f);
		repo.Register(unit);
		return unit;
	}

	private static Unit CreateUnit(bool isPlayer)
	{
		return UnitTestFactory.CreateUnit(AttackType.Melee, isPlayer);
	}
}

public class HealerBehaviorTests
{
	[Fact]
	public void UpdateTargeting_Healer_WithInjuredAlly_SetsHealingState()
	{
		FakeUnitRepository repo = new();
		Unit healer = CreateHealer(repo, 0f);
		Unit ally = CreateAlly(repo, 50f);
		ally.TakeDamage(20, null, 0f, 0f);

		healer.UpdateTargeting(null);

		Assert.Equal(UnitState.Healing, healer.State);
		Assert.Same(ally, healer.HealTarget);
		Assert.Null(healer.TargetEnemy);
	}

	[Fact]
	public void UpdateTargeting_Healer_NoInjuredAlly_Marches()
	{
		FakeUnitRepository repo = new();
		Unit healer = CreateHealer(repo, 0f);

		healer.UpdateTargeting(null);

		Assert.Equal(UnitState.Marching, healer.State);
		Assert.Null(healer.HealTarget);
	}

	[Fact]
	public void UpdateTargeting_Healer_IgnoresEnemyTarget()
	{
		FakeUnitRepository repo = new();
		Unit healer = CreateHealer(repo, 0f);
		Unit ally = CreateAlly(repo, 50f);
		ally.TakeDamage(20, null, 0f, 0f);
		Unit enemy = CreateEnemy(repo, 30f);

		healer.UpdateTargeting(enemy);

		Assert.Equal(UnitState.Healing, healer.State);
		Assert.Null(healer.TargetEnemy);
	}

	[Fact]
	public void UpdateBehavior_Healer_HealsAllyInRange()
	{
		FakeUnitRepository repo = new();
		Unit healer = CreateHealer(repo, 0f);
		Unit ally = CreateAlly(repo, 50f);
		ally.TakeDamage(50, null, 0f, 0f);
		healer.UpdateTargeting(null);

		healer.UpdateBehavior(0.2f, 500f, 500f);

		Assert.Equal(80, ally.Health);
		Assert.False(healer.HasMoveGoal);
	}

	[Fact]
	public void UpdateBehavior_Healer_RespectsHealInterval()
	{
		FakeUnitRepository repo = new();
		Unit healer = CreateHealer(repo, 0f);
		Unit ally = CreateAlly(repo, 50f);
		ally.TakeDamage(50, null, 0f, 0f);
		healer.UpdateTargeting(null);

		healer.UpdateBehavior(0.2f, 500f, 500f);
		healer.UpdateBehavior(0.2f, 500f, 500f);

		Assert.Equal(80, ally.Health);
	}

	[Fact]
	public void UpdateBehavior_Healer_AfterIntervalHealsAgain()
	{
		FakeUnitRepository repo = new();
		Unit healer = CreateHealer(repo, 0f);
		Unit ally = CreateAlly(repo, 50f);
		ally.TakeDamage(50, null, 0f, 0f);
		healer.UpdateTargeting(null);

		healer.UpdateBehavior(0.2f, 500f, 500f);
		healer.UpdateBehavior(2.2f, 500f, 500f);

		Assert.Equal(100, ally.Health);
	}

	[Fact]
	public void UpdateBehavior_Healer_NoTarget_Marches()
	{
		FakeUnitRepository repo = new();
		Unit healer = CreateHealer(repo, 0f);
		healer.UpdateTargeting(null);

		healer.UpdateBehavior(0.2f, 500f, 500f);

		Assert.True(healer.HasMoveGoal);
		Assert.Equal(500f, healer.MoveGoalX);
		Assert.Equal(500f, healer.MoveGoalY);
	}

	[Fact]
	public void UpdateBehavior_Healer_DoesNotAttackEnemy()
	{
		FakeUnitRepository repo = new();
		Unit healer = CreateHealer(repo, 0f);
		Unit ally = CreateAlly(repo, 50f);
		ally.TakeDamage(50, null, 0f, 0f);
		Unit enemy = CreateEnemy(repo, 40f);
		healer.UpdateTargeting(enemy);

		healer.UpdateBehavior(0.2f, 500f, 500f);

		Assert.Equal(100, enemy.Health);
	}

	private static Unit CreateHealer(FakeUnitRepository repo, float offsetX)
	{
		UnitStats stats = new()
		{
			TypeId = "Priest",
			Size = UnitSize.Medium,
			AttackType = AttackType.Melee,
			DamageType = DamageType.Holy,
			ArmorType = ArmorType.Plate,
			Health = 90,
			Damage = 0,
			Speed = 5f,
			AttackRange = 0f,
			AttackCooldown = 10f,
			VisionRange = 220f,
			UnitCost = 2600,
			Race = "Human",
			HealAmount = 30f,
			HealRange = 150f,
			HealInterval = 2f
		};
		Unit unit = new();
		unit.Initialize(stats, true);
		unit.SetDamageMatrix(new DamageMatrix());
		unit.SetPosition(offsetX, 0f);
		unit.Tactical = new TacticalQueryService(repo);
		repo.Register(unit);
		return unit;
	}

	private static Unit CreateAlly(FakeUnitRepository repo, float offsetX)
	{
		Unit unit = CreateUnit(true);
		unit.SetPosition(offsetX, 0f);
		repo.Register(unit);
		return unit;
	}

	private static Unit CreateEnemy(FakeUnitRepository repo, float offsetX)
	{
		Unit unit = CreateUnit(false);
		unit.SetPosition(offsetX, 0f);
		repo.Register(unit);
		return unit;
	}

	private static Unit CreateUnit(bool isPlayer)
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, isPlayer);
		return unit;
	}
}
