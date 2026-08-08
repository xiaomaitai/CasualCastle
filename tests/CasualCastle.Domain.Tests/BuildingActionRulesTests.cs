using CasualCastle.Domain.Building;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class BuildingActionRulesTests
{
	[Theory]
	[InlineData("DarkAltar", true)]
	[InlineData("RitualCircleL1", true)]
	[InlineData("RitualCircleL5", true)]
	[InlineData("Barracks", false)]
	[InlineData("DarkMageTower", false)]
	[InlineData(null, false)]
	public void CanUseSpellBar_RitualHostsOnly(string typeId, bool expected)
	{
		Assert.Equal(expected, BuildingActionRules.CanUseSpellBar(typeId));
	}
}
