using CasualCastle.Domain.Building;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class ShopTests
{
	private static Shop CreateShop(out Hand hand, params CardData[] catalog)
	{
		Player player = new Player();
		hand = new Hand(new FakeBuildingPlacement(), player);
		return new Shop(hand, new ShopRules(catalog), player);
	}

	[Fact]
	public void TryPurchase_EmptiesSlot_UntilRefresh()
	{
		Shop shop = CreateShop(out _, new CardData { Id = "a", Name = "A", Cost = 1, BuildingType = "Barracks", Weight = 1 });

		Assert.True(shop.TryPurchase(0));
		Assert.Null(shop.GetOffer(0));
		Assert.NotNull(shop.GetOffer(1));
		Assert.False(shop.TryPurchase(0));
	}

	[Fact]
	public void RefreshOffers_RefillsEmptySlot()
	{
		Shop shop = CreateShop(out _, new CardData { Id = "a", Name = "A", Cost = 1, BuildingType = "Barracks", Weight = 1 });
		shop.TryPurchase(0);

		shop.RefreshOffers();

		Assert.NotNull(shop.GetOffer(0));
	}

	[Fact]
	public void TryPlaceOfferDirect_EmptiesSlot()
	{
		Shop shop = CreateShop(out _, new CardData { Id = "a", Name = "A", Cost = 1, BuildingType = "Barracks", Weight = 1 });

		Assert.True(shop.TryPlaceOfferDirect(0, 1, 1));
		Assert.Null(shop.GetOffer(0));
		Assert.False(shop.TryPlaceOfferDirect(0, 1, 1));
	}
}
