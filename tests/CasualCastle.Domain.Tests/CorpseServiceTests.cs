using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class CorpseServiceTests
{
	[Fact]
	public void ScheduleCorpseRemoval_KeepsCorpseUntilScheduledTime()
	{
		FakeDelayScheduler scheduler = new();
		FakeUnitRepository repo = new();
		Unit corpse = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		corpse.IsAlive = false;
		repo.Register(corpse);
		CorpseService service = new(scheduler, repo);

		service.ScheduleCorpseRemoval(corpse);

		Assert.Contains(corpse, repo.AllUnits);
		scheduler.FireAll();
		Assert.DoesNotContain(corpse, repo.AllUnits);
	}

	[Fact]
	public void ScheduleCorpseRemoval_UsesRetentionDuration()
	{
		FakeDelayScheduler scheduler = new();
		FakeUnitRepository repo = new();
		Unit corpse = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		CorpseService service = new(scheduler, repo);

		service.ScheduleCorpseRemoval(corpse);

		Assert.Equal(CorpseRules.RetentionSeconds, scheduler.LastDelaySeconds);
	}

	[Fact]
	public void ScheduleCorpseRemoval_RemovesOnlyScheduledCorpse()
	{
		FakeDelayScheduler scheduler = new();
		FakeUnitRepository repo = new();
		Unit corpse = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		Unit other = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		repo.Register(corpse);
		repo.Register(other);
		CorpseService service = new(scheduler, repo);

		service.ScheduleCorpseRemoval(corpse);
		scheduler.FireAll();

		Assert.Contains(other, repo.AllUnits);
		Assert.DoesNotContain(corpse, repo.AllUnits);
	}
}
