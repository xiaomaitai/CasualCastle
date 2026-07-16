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
}
