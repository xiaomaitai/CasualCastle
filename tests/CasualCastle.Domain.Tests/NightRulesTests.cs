using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class NightRulesTests
{
	[Fact]
	public void CanUnitWork_Day_ReturnsTrueAlways()
	{
		Assert.True(NightRules.CanUnitWork(false, true));
		Assert.True(NightRules.CanUnitWork(true, true));
	}

	[Fact]
	public void CanUnitWork_Night_ReturnsTrueOnlyWithNightCombat()
	{
		Assert.True(NightRules.CanUnitWork(true, false));
		Assert.False(NightRules.CanUnitWork(false, false));
	}
}
