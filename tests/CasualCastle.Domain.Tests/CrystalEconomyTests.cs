using System.Collections.Generic;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class CrystalEconomyTests
{
	[Fact]
	public void Shop_TryPurchaseCrystal_AddsCardToHandAndSpendsGold()
	{
		CardData crystal = new()
		{
			Id = "DungeonCrystal",
			Name = "经验魔晶",
			Cost = 6,
			Weight = 8,
			FactionId = "dungeon",
			ItemType = ShopItemTypes.DungeonCrystal,
		};
		ShopRules rules = new(new List<CardData> { crystal });
		Player player = new();
		Hand hand = new(new FakeBuildingPlacement(), player);
		Shop shop = new(hand, rules, player);
		shop.RefreshOffers();
		int slot = FindCrystalSlot(shop);
		Assert.True(slot >= 0);

		int goldBefore = shop.Gold;
		bool ok = shop.TryPurchase(slot);

		Assert.True(ok);
		Assert.Equal(goldBefore - 6, shop.Gold);
		Assert.Single(hand.Cards);
		Assert.Equal("DungeonCrystal", hand.Cards[0].Id);
	}

	[Fact]
	public void KingExperience_OnSmallCrystalMined_GrantsSmallXp()
	{
		KingExperienceService xp = new();

		xp.OnSmallCrystalMined();

		Assert.Equal(DemonKingRules.SmallCrystalXp, xp.Experience);
	}

	private static int FindCrystalSlot(Shop shop)
	{
		for (int i = 0; i < Shop.OfferCount; i++)
		{
			if (shop.GetOffer(i)?.Id == "DungeonCrystal")
				return i;
		}
		return -1;
	}
}