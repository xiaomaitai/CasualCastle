using System.Collections.Generic;
using CasualCastle.Domain.Building;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class DarkMagicBookRulesTests
{
	[Fact]
	public void GetRitualId_DarkMagicBookWithRitualAttribute_ReturnsRitual()
	{
		CardData book = new()
		{
			Id = "book_summon",
			ItemType = ShopItemTypes.DarkMagicBook,
			Attributes = new() { { DarkMagicBookRules.RitualAttributeKey, RitualRules.UndeadSummonId } },
		};

		Assert.Equal(RitualRules.UndeadSummonId, DarkMagicBookRules.GetRitualId(book));
		Assert.True(DarkMagicBookRules.IsDarkMagicBook(book));
	}

	[Fact]
	public void GetRitualId_OtherItemTypeOrBuildingCard_ReturnsNull()
	{
		CardData otherItem = new()
		{
			Id = "book",
			ItemType = "future_item",
			Attributes = new() { { DarkMagicBookRules.RitualAttributeKey, RitualRules.UndeadSummonId } },
		};
		CardData buildingCard = new() { Id = "book", BuildingType = "Tomb" };

		Assert.Null(DarkMagicBookRules.GetRitualId(otherItem));
		Assert.Null(DarkMagicBookRules.GetRitualId(buildingCard));
		Assert.Null(DarkMagicBookRules.GetRitualId(null));
	}

	[Fact]
	public void GetRitualId_BookWithoutRitualAttribute_ReturnsNull()
	{
		CardData book = new() { Id = "book_empty", ItemType = ShopItemTypes.DarkMagicBook };

		Assert.Null(DarkMagicBookRules.GetRitualId(book));
		Assert.False(DarkMagicBookRules.IsDarkMagicBook(book));
	}
}