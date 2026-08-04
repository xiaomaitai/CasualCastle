using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class StealthServiceTests
{
	private static Unit CreateStealthyUnit(bool isPlayerUnit, float gameX, float gameY)
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, isPlayerUnit);
		unit.Skills.Add(UnitTestFactory.CreateStealthSkill(180f, 0.85f));
		unit.SetPosition(gameX, gameY);
		return unit;
	}

	[Fact]
	public void UpdateStealth_NoEnemyNearby_EntersStealth()
	{
		FakeUnitRepository repo = new();
		Unit assassin = CreateStealthyUnit(true, 0, 0);
		Unit enemy = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		enemy.SetPosition(1000, 0);
		repo.Register(assassin);
		repo.Register(enemy);

		new StealthService().UpdateStealth(assassin, repo);

		Assert.True(assassin.IsStealthed);
	}

	[Fact]
	public void UpdateStealth_EnemyWithinEngageDistance_DoesNotEnterStealth()
	{
		FakeUnitRepository repo = new();
		Unit assassin = CreateStealthyUnit(true, 0, 0);
		Unit enemy = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		enemy.SetPosition(100, 0);
		repo.Register(assassin);
		repo.Register(enemy);

		new StealthService().UpdateStealth(assassin, repo);

		Assert.False(assassin.IsStealthed);
	}

	[Fact]
	public void UpdateStealth_StealthedWithNearbyEnemy_StaysStealthed()
	{
		FakeUnitRepository repo = new();
		Unit assassin = CreateStealthyUnit(true, 0, 0);
		Unit enemy = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		enemy.SetPosition(50, 0);
		repo.Register(assassin);
		repo.Register(enemy);
		assassin.SetStealth(true);

		new StealthService().UpdateStealth(assassin, repo);

		Assert.True(assassin.IsStealthed);
	}

	[Fact]
	public void UpdateStealth_EnemyDetectorAlive_CannotEnterStealth()
	{
		FakeUnitRepository repo = new();
		Unit assassin = CreateStealthyUnit(true, 0, 0);
		Unit detector = UnitTestFactory.CreateUnit(AttackType.Melee, false, 90f, null, true);
		detector.SetPosition(1000, 0);
		repo.Register(assassin);
		repo.Register(detector);

		new StealthService().UpdateStealth(assassin, repo);

		Assert.False(assassin.IsStealthed);
	}

	[Fact]
	public void UpdateStealth_DetectorAppearsWhileStealthed_ExitsStealth()
	{
		FakeUnitRepository repo = new();
		Unit assassin = CreateStealthyUnit(true, 0, 0);
		Unit detector = UnitTestFactory.CreateUnit(AttackType.Melee, false, 90f, null, true);
		detector.SetPosition(1000, 0);
		repo.Register(assassin);
		repo.Register(detector);
		assassin.SetStealth(true);

		new StealthService().UpdateStealth(assassin, repo);

		Assert.False(assassin.IsStealthed);
	}

	[Fact]
	public void UpdateStealth_UnitWithoutStealthSkill_NeverStealthed()
	{
		FakeUnitRepository repo = new();
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		repo.Register(unit);

		new StealthService().UpdateStealth(unit, repo);

		Assert.False(unit.IsStealthed);
	}

	[Fact]
	public void UpdateStealth_EnterStealth_ClearsEnemyTargeting()
	{
		FakeUnitRepository repo = new();
		Unit assassin = CreateStealthyUnit(true, 0, 0);
		Unit enemy = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		enemy.SetPosition(1000, 0);
		enemy.SetEnemyTarget(assassin);
		repo.Register(assassin);
		repo.Register(enemy);

		new StealthService().UpdateStealth(assassin, repo);

		Assert.Null(enemy.TargetEnemy);
	}
}
