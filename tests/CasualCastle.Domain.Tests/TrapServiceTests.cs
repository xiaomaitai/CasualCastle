using CasualCastle.Domain.Battle;
using System.Collections.Generic;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class TrapServiceTests
{
	[Fact]
	public void SpikeTrap_DamagesEnemiesInRange()
	{
		Unit source = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		target.SetPosition(10f, 0f);
		FakeTacticalQueries tactical = new();
		tactical.Enemies.Add(target);
		TrapService service = new(tactical, new DamageMatrix());

		int hits = service.Trigger(source, TrapRules.SpikeTrap, 0f, 0f, 0f, 0f);

		Assert.Equal(1, hits);
		Assert.Equal(75, target.Health);
	}

	[Fact]
	public void SpikeTrap_IgnoresEnemiesOutOfRange()
	{
		Unit source = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		target.SetPosition(500f, 0f);
		FakeTacticalQueries tactical = new();
		tactical.Enemies.Add(target);
		TrapService service = new(tactical, new DamageMatrix());

		int hits = service.Trigger(source, TrapRules.SpikeTrap, 0f, 0f, 0f, 0f);

		Assert.Equal(0, hits);
		Assert.Equal(100, target.Health);
	}

	[Fact]
	public void PoisonTrap_AppliesPoisonOverTime()
	{
		Unit source = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		target.SetPosition(10f, 0f);
		FakeTacticalQueries tactical = new();
		tactical.Enemies.Add(target);
		TrapService service = new(tactical, new DamageMatrix());

		service.Trigger(source, TrapRules.PoisonTrap, 0f, 0f, 0f, 0f);
		target.UpdateBehavior(1f, 0f, 0f);

		Assert.True(target.Health < 85);
	}

	[Fact]
	public void TeleportTrap_MovesEnemyToTargetPosition()
	{
		Unit source = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		target.SetPosition(10f, 0f);
		FakeTacticalQueries tactical = new();
		tactical.Enemies.Add(target);
		TrapService service = new(tactical, new DamageMatrix());

		service.Trigger(source, TrapRules.TeleportTrap, 0f, 0f, 800f, 600f);

		Assert.Equal(800f, target.GameX);
		Assert.Equal(600f, target.GameY);
		Assert.Equal(100, target.Health);
	}

	[Fact]
	public void Trigger_IgnoresDeadEnemies()
	{
		Unit source = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		target.SetPosition(10f, 0f);
		target.TakeDamage(100, source, 0f, 0f);
		FakeTacticalQueries tactical = new();
		tactical.Enemies.Add(target);
		TrapService service = new(tactical, new DamageMatrix());

		int hits = service.Trigger(source, TrapRules.SpikeTrap, 0f, 0f, 0f, 0f);

		Assert.Equal(0, hits);
	}
}