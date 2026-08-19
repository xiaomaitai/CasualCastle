using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class StatusEffectsTests
{
	[Fact]
	public void ApplySlow_ReducesCurrentSpeed()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		unit.Speed = 10f;

		unit.ApplySlow(0.5f, 5f);

		Assert.Equal(5f, unit.GetCurrentSpeed(), 3);
	}

	[Fact]
	public void ApplySlow_ExpiresAfterDuration()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		unit.Speed = 10f;
		unit.ApplySlow(0.5f, 0.5f);

		unit.UpdateBehavior(0.6f, 0, 0);

		Assert.Equal(10f, unit.GetCurrentSpeed(), 3);
	}

	[Fact]
	public void ApplyPoison_DealsDamageOverTime()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		int initial = unit.Health;

		unit.ApplyPoison(10f, 1f);
		unit.UpdateBehavior(1f, 0, 0);

		Assert.True(unit.IsAlive);
		Assert.True(unit.Health < initial);
	}
}
