using System;
using CasualCastle.Domain.Building;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class ShopRulesTests
{
	[Fact]
	public void GenerateOffers_ReturnsCorrectNumberOfOffers()
	{
		CardData[] catalog = new[]
		{
			new CardData { Id = "a", Name = "A", Cost = 10, BuildingType = "Barracks", Weight = 1 },
		};
		ShopRules rules = new(catalog);
		CardData[] offers = rules.GenerateOffers(new Random(42));

		Assert.Equal(ShopRules.OfferCount, offers.Length);
		Assert.Equal("a", offers[0].Id);
	}

	[Fact]
	public void GenerateOffers_WithSeededRandom_ReturnsDeterministicResults()
	{
		CardData[] catalog = new[]
		{
			new CardData { Id = "a", Name = "A", Cost = 10, BuildingType = "Barracks", Weight = 1 },
			new CardData { Id = "b", Name = "B", Cost = 20, BuildingType = "Archery", Weight = 2 },
		};
		ShopRules rules = new(catalog);
		CardData[] offers1 = rules.GenerateOffers(new Random(42));
		CardData[] offers2 = rules.GenerateOffers(new Random(42));

		for (int i = 0; i < ShopRules.OfferCount; i++)
			Assert.Equal(offers1[i].Id, offers2[i].Id);
	}

	[Fact]
	public void RefreshOfferSlot_ReturnsCardFromCatalog()
	{
		CardData[] catalog = new[]
		{
			new CardData { Id = "a", Name = "A", Cost = 10, BuildingType = "Barracks", Weight = 1 },
		};
		ShopRules rules = new(catalog);
		CardData card = rules.RefreshOfferSlot(new Random(42));

		Assert.NotNull(card);
		Assert.Equal("a", card.Id);
	}

	[Fact]
	public void SetFaction_Horde_OnlyOffersHordeAndNeutralCards()
	{
		CardData[] catalog = new[]
		{
			new CardData { Id = "a", Name = "A", Cost = 10, BuildingType = "Barracks", Weight = 1, FactionId = "alliance" },
			new CardData { Id = "b", Name = "B", Cost = 10, BuildingType = "OrcCamp", Weight = 1, FactionId = FactionDef.HordeId },
			new CardData { Id = "c", Name = "C", Cost = 10, BuildingType = "Barracks", Weight = 1, FactionId = "" },
		};
		ShopRules rules = new(catalog);
		rules.SetFaction(FactionDef.HordeId);

		CardData[] offers = rules.GenerateOffers(new Random(42));

		Assert.All(offers, card => Assert.True(card.Id == "b" || card.Id == "c"));
	}

	[Fact]
	public void SetFaction_Alliance_OnlyOffersAllianceAndNeutralCards()
	{
		CardData[] catalog = new[]
		{
			new CardData { Id = "a", Name = "A", Cost = 10, BuildingType = "Barracks", Weight = 1, FactionId = "alliance" },
			new CardData { Id = "b", Name = "B", Cost = 10, BuildingType = "OrcCamp", Weight = 1, FactionId = FactionDef.HordeId },
			new CardData { Id = "c", Name = "C", Cost = 10, BuildingType = "Barracks", Weight = 1, FactionId = "" },
		};
		ShopRules rules = new(catalog);
		rules.SetFaction("alliance");

		CardData[] offers = rules.GenerateOffers(new Random(42));

		Assert.All(offers, card => Assert.True(card.Id == "a" || card.Id == "c"));
	}

	[Fact]
	public void SetFaction_ItemCards_FilterByFaction()
	{
		CardData[] catalog = new[]
		{
			new CardData { Id = "horde_item", Name = "H", Cost = 4, BuildingType = "", Weight = 1, FactionId = "horde", ItemType = "horde_totem" },
			new CardData { Id = "alliance_item", Name = "A", Cost = 4, BuildingType = "", Weight = 1, FactionId = "alliance", ItemType = "scroll" },
			new CardData { Id = "neutral_item", Name = "N", Cost = 4, BuildingType = "", Weight = 1, FactionId = "", ItemType = "relic" },
		};
		ShopRules rules = new(catalog);
		rules.SetFaction("horde");

		CardData[] offers = rules.GenerateOffers(new Random(42));

		Assert.All(offers, card => Assert.True(card.Id == "horde_item" || card.Id == "neutral_item"));
	}

	[Fact]
	public void SetFaction_Empty_KeepsFullCatalog()
	{
		CardData[] catalog = new[]
		{
			new CardData { Id = "a", Name = "A", Cost = 10, BuildingType = "Barracks", Weight = 1, FactionId = "alliance" },
			new CardData { Id = "b", Name = "B", Cost = 10, BuildingType = "OrcCamp", Weight = 1, FactionId = FactionDef.HordeId },
		};
		ShopRules rules = new(catalog);

		CardData[] offers = rules.GenerateOffers(new Random(42));

		Assert.All(offers, card => Assert.True(card.Id == "a" || card.Id == "b"));
	}
}
