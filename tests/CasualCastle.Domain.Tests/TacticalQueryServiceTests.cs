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
}
