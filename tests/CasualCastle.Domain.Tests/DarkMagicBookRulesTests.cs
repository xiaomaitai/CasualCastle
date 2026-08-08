using System.Collections.Generic;
using CasualCastle.Domain.Building;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class DarkMagicBookRulesTests
{
	[Fact]
	public void GetRitualId_DarkMagicBookCard_ReturnsMappedRitual()
	{
		DarkMagicBookRules.Load(new List<KeyValuePair<string, string>>
		{
			new("book_summon", RitualRules.UndeadSummonId),
			new("book_reinforce", RitualRules.BoneReinforceId)
		});

		CardData summonBook = new() { Id = "book_summon", ItemType = ShopItemTypes.DarkMagicBook };
		CardData reinforceBook = new() { Id = "book_reinforce", ItemType = ShopItemTypes.DarkMagicBook };

		Assert.Equal(RitualRules.UndeadSummonId, DarkMagicBookRules.GetRitualId(summonBook));
		Assert.True(DarkMagicBookRules.IsDarkMagicBook(reinforceBook));
	}

	[Fact]
	public void GetRitualId_OtherItemTypeOrBuildingCard_ReturnsNull()
	{
		DarkMagicBookRules.Load(new List<KeyValuePair<string, string>> { new("book_summon", RitualRules.UndeadSummonId) });

		CardData otherItem = new() { Id = "book_summon", ItemType = "future_item" };
		CardData buildingCard = new() { Id = "book_summon", BuildingType = "Tomb" };

		Assert.Null(DarkMagicBookRules.GetRitualId(otherItem));
		Assert.Null(DarkMagicBookRules.GetRitualId(buildingCard));
		Assert.Null(DarkMagicBookRules.GetRitualId(null));
	}

	[Fact]
	public void Load_ReplacesPreviousMapping()
	{
		DarkMagicBookRules.Load(new List<KeyValuePair<string, string>> { new("book_a", "ritual_a") });
		DarkMagicBookRules.Load(new List<KeyValuePair<string, string>> { new("book_b", "ritual_b") });

		CardData bookA = new() { Id = "book_a", ItemType = ShopItemTypes.DarkMagicBook };
		CardData bookB = new() { Id = "book_b", ItemType = ShopItemTypes.DarkMagicBook };

		Assert.Null(DarkMagicBookRules.GetRitualId(bookA));
		Assert.Equal("ritual_b", DarkMagicBookRules.GetRitualId(bookB));
	}
}