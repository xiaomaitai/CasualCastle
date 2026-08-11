using System;
using System.Collections.Generic;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class ShopBookAvailabilityTests
{
	[Fact]
	public void ShopRules_BookHeld_NotOffered()
	{
		CardData[] catalog = BuildCatalog();
		ShopRules rules = new(catalog);
		rules.SetAvailability(_ => false, cardId => cardId == "DarkMagicBook_UndeadSummon", () => 1);

		CardData[] offers = rules.GenerateOffers(new Random(42));

		Assert.DoesNotContain(offers, card => card != null && card.Id == "DarkMagicBook_UndeadSummon");
	}

	[Fact]
	public void ShopRules_BookNotHeld_Offered()
	{
		CardData book = CreateBook();
		ShopRules rules = new(new[] { book });
		rules.SetAvailability(_ => false, _ => false, () => 1);

		CardData[] offers = rules.GenerateOffers(new Random(42));

		Assert.Equal("DarkMagicBook_UndeadSummon", offers[0]?.Id);
	}

	[Fact]
	public void ShopRules_LearnedBook_NotOffered()
	{
		CardData[] catalog = BuildCatalog();
		ShopRules rules = new(catalog);
		rules.SetAvailability(ritualId => ritualId == RitualRules.UndeadSummonId, _ => false, () => 1);

		CardData[] offers = rules.GenerateOffers(new Random(42));

		Assert.DoesNotContain(offers, card => card != null && card.Id == "DarkMagicBook_UndeadSummon");
	}

	[Fact]
	public void ShopRules_Book_AtMostOneCopyPerRefresh()
	{
		CardData[] catalog = BuildCatalog();
		ShopRules rules = new(catalog);
		rules.SetAvailability(_ => false, _ => false, () => 1);
		Random random = new(42);

		for (int i = 0; i < 20; i++)
		{
			CardData[] offers = rules.GenerateOffers(random);
			int count = 0;
			foreach (CardData card in offers)
			{
				if (card != null && card.Id == "DarkMagicBook_UndeadSummon")
					count++;
			}
			Assert.True(count <= 1);
		}
	}

	[Fact]
	public void ShopRules_DarkAltar_NotOfferedUntilEvilRitualLearned()
	{
		CardData[] catalog = new[]
		{
			new CardData { Id = "tomb", Name = "墓穴", Cost = 2, BuildingType = "Tomb", Weight = 10, FactionId = FactionIds.Necromancer },
			new CardData { Id = "dark_altar", Name = "黑暗祭坛", Cost = 3, BuildingType = "DarkAltar", Weight = 8, FactionId = FactionIds.Necromancer, UnlockRitualId = RitualRules.EvilRitualId },
		};
		ShopRules locked = new(catalog);
		locked.SetAvailability(_ => false, _ => false, () => 1);

		Assert.DoesNotContain(locked.GenerateOffers(new Random(42)), card => card != null && card.Id == "dark_altar");

		ShopRules unlocked = new(catalog);
		unlocked.SetAvailability(ritualId => ritualId == RitualRules.EvilRitualId, _ => false, () => 1);

		Assert.Contains(unlocked.GenerateOffers(new Random(42)), card => card != null && card.Id == "dark_altar");
	}

	[Fact]
	public void ShopRules_BookAboveCoreLevel_NotOffered()
	{
		CardData[] catalog = new[]
		{
			new CardData { Id = "tomb", Name = "墓穴", Cost = 2, BuildingType = "Tomb", Weight = 10, FactionId = FactionIds.Necromancer },
			new CardData { Id = "DarkMagicBook_BoneReinforce", Name = "骸骨强化之书", Cost = 4, BuildingType = "", Weight = 6, FactionId = FactionIds.Necromancer, ItemType = ShopItemTypes.DarkMagicBook, Attributes = new() { { DarkMagicBookRules.RitualAttributeKey, RitualRules.BoneReinforceId } } },
		};
		ShopRules lowCore = new(catalog);
		lowCore.SetAvailability(_ => false, _ => false, () => 1);

		Assert.DoesNotContain(lowCore.GenerateOffers(new Random(42)), card => card != null && card.Id == "DarkMagicBook_BoneReinforce");

		ShopRules highCore = new(catalog);
		highCore.SetAvailability(_ => false, _ => false, () => 2);

		Assert.Contains(highCore.GenerateOffers(new Random(42)), card => card != null && card.Id == "DarkMagicBook_BoneReinforce");
	}

	[Fact]
	public void Shop_PurchasedBook_NotOfferedAfterRefresh_UntilSold()
	{
		CardData book = CreateBook();
		Player player = new();
		Hand hand = new(new FakeBuildingPlacement(), player);
		ShopRules rules = new(new[] { book });
		rules.SetAvailability(_ => false, cardId => IsBookInHand(hand, cardId), () => 1);
		Shop shop = new(hand, rules, player);
		shop.SetFaction(FactionIds.Necromancer);

		Assert.Equal(0, FindSlot(shop, "DarkMagicBook_UndeadSummon"));
		Assert.True(shop.TryPurchase(0));

		shop.RefreshOffers();
		Assert.Equal(-1, FindSlot(shop, "DarkMagicBook_UndeadSummon"));

		Assert.True(shop.TrySellHandCard(0));
		shop.RefreshOffers();
		Assert.Equal(0, FindSlot(shop, "DarkMagicBook_UndeadSummon"));
	}

	private static CardData CreateBook()
	{
		return new CardData { Id = "DarkMagicBook_UndeadSummon", Name = "亡灵召唤之书", Cost = 4, BuildingType = "", Weight = 5, FactionId = FactionIds.Necromancer, ItemType = ShopItemTypes.DarkMagicBook, Attributes = new() { { DarkMagicBookRules.RitualAttributeKey, RitualRules.UndeadSummonId } } };
	}

	private static CardData[] BuildCatalog()
	{
		return new[]
		{
			new CardData { Id = "tomb", Name = "墓穴", Cost = 2, BuildingType = "Tomb", Weight = 10, FactionId = FactionIds.Necromancer },
			new CardData { Id = "blight", Name = "腐化之地", Cost = 2, BuildingType = "BlightField", Weight = 8, FactionId = FactionIds.Necromancer },
			new CardData { Id = "DarkMagicBook_UndeadSummon", Name = "亡灵召唤之书", Cost = 4, BuildingType = "", Weight = 5, FactionId = FactionIds.Necromancer, ItemType = ShopItemTypes.DarkMagicBook, Attributes = new() { { DarkMagicBookRules.RitualAttributeKey, RitualRules.UndeadSummonId } } },
		};
	}

	private static int FindSlot(Shop shop, string cardId)
	{
		for (int i = 0; i < Shop.OfferCount; i++)
		{
			CardData offer = shop.GetOffer(i);
			if (offer != null && offer.Id == cardId)
				return i;
		}
		return -1;
	}

	private static bool IsBookInHand(Hand hand, string cardId)
	{
		foreach (CardData card in hand.Cards)
		{
			if (card.Id == cardId && DarkMagicBookRules.IsDarkMagicBook(card))
				return true;
		}
		return false;
	}
}
