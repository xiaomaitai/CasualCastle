using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class TacticalQueryServiceTests
{
	[Fact]
	public void FindNearestEnemy_MeleeUnit_ReturnsClosestEnemyInVision()
	{
		FakeUnitRepository repo = new();
		Unit self = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		Unit near = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		Unit far = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		self.SetPosition(0, 0);
		near.SetPosition(100, 0);
		far.SetPosition(200, 0);
		repo.Register(self);
		repo.Register(near);
		repo.Register(far);

		(IUnitHandle nearest, _) = new TacticalQueryService(repo).FindNearestEnemy(self);

		Assert.Same(near, nearest);
	}

	[Fact]
	public void FindNearestEnemy_EnemyOutsideVision_ReturnsNull()
	{
		FakeUnitRepository repo = new();
		Unit self = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		Unit far = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		self.SetPosition(0, 0);
		far.SetPosition(1000, 0);
		repo.Register(self);
		repo.Register(far);

		(IUnitHandle nearest, _) = new TacticalQueryService(repo).FindNearestEnemy(self);

		Assert.Null(nearest);
	}

	[Fact]
	public void FindNearestEnemy_RangedUnit_StillPrefersNearestEnemy()
	{
		FakeUnitRepository repo = new();
		Unit self = UnitTestFactory.CreateUnit(AttackType.Ranged, true);
		Unit melee = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		Unit ranged = UnitTestFactory.CreateUnit(AttackType.Ranged, false);
		self.SetPosition(0, 0);
		melee.SetPosition(100, 0);
		ranged.SetPosition(200, 0);
		repo.Register(self);
		repo.Register(melee);
		repo.Register(ranged);

		(IUnitHandle nearest, _) = new TacticalQueryService(repo).FindNearestEnemy(self);

		Assert.Same(melee, nearest);
	}

	[Fact]
	public void FindNearestEnemy_StealthedEnemy_NotSelectedAsTarget()
	{
		FakeUnitRepository repo = new();
		Unit self = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		Unit stealthed = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		self.SetPosition(0, 0);
		stealthed.SetPosition(100, 0);
		stealthed.SetStealth(true);
		repo.Register(self);
		repo.Register(stealthed);

		(IUnitHandle nearest, _) = new TacticalQueryService(repo).FindNearestEnemy(self);

		Assert.Null(nearest);
	}

	[Fact]
	public void FindNearestEnemy_DetectorUnit_CanSelectStealthedEnemy()
	{
		FakeUnitRepository repo = new();
		Unit self = UnitTestFactory.CreateUnit(AttackType.Melee, true, 90f, null, true);
		Unit stealthed = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		self.SetPosition(0, 0);
		stealthed.SetPosition(100, 0);
		stealthed.SetStealth(true);
		repo.Register(self);
		repo.Register(stealthed);

		(IUnitHandle nearest, _) = new TacticalQueryService(repo).FindNearestEnemy(self);

		Assert.Same(stealthed, nearest);
	}

	[Fact]
	public void FindNearestEnemy_StealthedAssassin_PrefersRangedEnemy()
	{
		FakeUnitRepository repo = new();
		Unit assassin = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		assassin.Skills.Add(UnitTestFactory.CreateStealthSkill(180f, 0.85f));
		assassin.SetStealth(true);
		Unit melee = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		Unit ranged = UnitTestFactory.CreateUnit(AttackType.Ranged, false);
		assassin.SetPosition(0, 0);
		melee.SetPosition(80, 0);
		ranged.SetPosition(150, 0);
		repo.Register(assassin);
		repo.Register(melee);
		repo.Register(ranged);

		(IUnitHandle nearest, _) = new TacticalQueryService(repo).FindNearestEnemy(assassin);

		Assert.Same(ranged, nearest);
	}
}
