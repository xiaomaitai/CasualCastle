using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class TrapCapTests
{
	[Fact]
	public void GetMaxTrapCount_BaseIsFour()
	{
		Assert.Equal(4, TrapRules.GetMaxTrapCount(false));
	}

	[Fact]
	public void GetMaxTrapCount_DoubledIsEight()
	{
		Assert.Equal(8, TrapRules.GetMaxTrapCount(true));
	}
}