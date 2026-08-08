using CasualCastle.Domain.Building;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class SellRulesTests
{
	[Fact]
	public void GetSellPrice_EvenValue_ReturnsHalf()
	{
		Assert.Equal(1, SellRules.GetSellPrice(2));
		Assert.Equal(2, SellRules.GetSellPrice(4));
	}

	[Fact]
	public void GetSellPrice_OddValue_RoundsUp()
	{
		Assert.Equal(1, SellRules.GetSellPrice(1));
		Assert.Equal(2, SellRules.GetSellPrice(3));
		Assert.Equal(3, SellRules.GetSellPrice(5));
	}
}