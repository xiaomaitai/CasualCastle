using CasualCastle.Domain.Battle;
using System.Linq;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class StatusEffectsActiveEffectsTests
{
	[Fact]
	public void GetActiveEffects_InitiallyEmpty()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);

		Assert.Empty(unit.GetActiveStatusEffects());
	}

	[Fact]
	public void GetActiveEffects_ReportsSlowWithRemaining()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);

		unit.ApplySlow(0.5f, 5f);

		ActiveStatusEffect effect = Assert.Single(unit.GetActiveStatusEffects());
		Assert.Equal(StatusEffects.SlowKind, effect.Kind);
		Assert.Equal(5f, effect.RemainingSeconds, 3);
	}

	[Fact]
	public void GetActiveEffects_ReportsPoisonAndCurse()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);

		unit.ApplyPoison(10f, 3f);
		unit.ApplyCurse(0.2f, 4f);

		ActiveStatusEffect[] effects = unit.GetActiveStatusEffects().OrderBy(e => e.Kind).ToArray();
		Assert.Equal(2, effects.Length);
		Assert.Contains(effects, e => e.Kind == StatusEffects.PoisonKind && e.RemainingSeconds == 3f);
		Assert.Contains(effects, e => e.Kind == StatusEffects.CurseKind && e.RemainingSeconds == 4f);
	}

	[Fact]
	public void GetActiveEffects_ClearsAfterExpiry()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		unit.ApplySlow(0.5f, 0.5f);
		unit.ApplyCurse(0.2f, 0.5f);

		unit.UpdateBehavior(0.6f, 0, 0);

		Assert.Empty(unit.GetActiveStatusEffects());
	}
}
