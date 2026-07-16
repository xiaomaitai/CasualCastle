using CasualCastle.Domain.Building;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class CardRulesTests
{
	[Fact]
	public void CanAddCard_BelowMax_ReturnsTrue()
	{
		Assert.True(CardRules.CanAddCard(0));
		Assert.True(CardRules.CanAddCard(6));
	}

	[Fact]
	public void CanAddCard_AtMax_ReturnsFalse()
	{
		Assert.False(CardRules.CanAddCard(CardRules.MaxHandSize));
		Assert.False(CardRules.CanAddCard(10));
	}

	[Fact]
	public void IsValidHandIndex_ValidIndex_ReturnsTrue()
	{
		Assert.True(CardRules.IsValidHandIndex(0, 3));
		Assert.True(CardRules.IsValidHandIndex(2, 3));
	}

	[Fact]
	public void IsValidHandIndex_InvalidIndex_ReturnsFalse()
	{
		Assert.False(CardRules.IsValidHandIndex(-1, 3));
		Assert.False(CardRules.IsValidHandIndex(3, 3));
		Assert.False(CardRules.IsValidHandIndex(5, 3));
	}
}
