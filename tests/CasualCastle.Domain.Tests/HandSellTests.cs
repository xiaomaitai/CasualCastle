using CasualCastle.Domain.Building;
using Xunit;

namespace CasualCastle.Domain.Tests;

internal sealed class FakeBuildingPlacement : IBuildingPlacement
{
	public int TryPlaceCalls;

	public bool CanPlace(string buildingType, int gridX, int gridY)
	{
		return true;
	}

	public bool TryPlace(string buildingType, int gridX, int gridY)
	{
		TryPlaceCalls++;
		return true;
	}
}

public class HandSellTests
{
	private static Hand CreateHand(out FakeBuildingPlacement placement)
	{
		placement = new FakeBuildingPlacement();
		return new Hand(placement, new Player());
	}

	[Fact]
	public void TryPlaceAtIndex_NonPlaceableCard_ReturnsFalseAndKeepsCard()
	{
		Hand hand = CreateHand(out FakeBuildingPlacement placement);
		hand.TryAddCard(new CardData { Id = "book", Cost = 4, BuildingType = "DarkMagicBook", ItemType = ShopItemTypes.DarkMagicBook });

		bool placed = hand.TryPlaceAtIndex(0, 1, 1);

		Assert.False(placed);
		Assert.Equal(0, placement.TryPlaceCalls);
		Assert.Single(hand.Cards);
	}

	[Fact]
	public void TryPlaceAtIndex_PlaceableCard_PlacesAndRemoves()
	{
		Hand hand = CreateHand(out FakeBuildingPlacement placement);
		hand.TryAddCard(new CardData { Id = "tomb", Cost = 2, BuildingType = "Tomb" });

		bool placed = hand.TryPlaceAtIndex(0, 1, 1);

		Assert.True(placed);
		Assert.Equal(1, placement.TryPlaceCalls);
		Assert.Empty(hand.Cards);
	}

	[Fact]
	public void TryRemoveAtIndex_RemovesCardAndFiresEvent()
	{
		Hand hand = CreateHand(out _);
		hand.TryAddCard(new CardData { Id = "book", Cost = 4, ItemType = ShopItemTypes.DarkMagicBook });
		int changes = 0;
		hand.HandChanged += () => changes++;

		bool removed = hand.TryRemoveAtIndex(0, out CardData card);

		Assert.True(removed);
		Assert.Equal("book", card.Id);
		Assert.Empty(hand.Cards);
		Assert.Equal(1, changes);
	}

	[Fact]
	public void TryRemoveCardById_RemovesMatchingCard()
	{
		Hand hand = CreateHand(out _);
		hand.TryAddCard(new CardData { Id = "book", Cost = 4, ItemType = ShopItemTypes.DarkMagicBook });
		hand.TryAddCard(new CardData { Id = "tomb", Cost = 2 });

		bool removed = hand.TryRemoveCardById("book");

		Assert.True(removed);
		Assert.Single(hand.Cards);
		Assert.Equal("tomb", hand.Cards[0].Id);
	}

	[Fact]
	public void ShopSellHandCard_RefundsCeiledHalfAndRemovesCard()
	{
		Hand hand = CreateHand(out _);
		CardData[] catalog = new[]
		{
			new CardData { Id = "book", Name = "黑暗魔法书", Cost = 5, BuildingType = "DarkMagicBook", ItemType = ShopItemTypes.DarkMagicBook },
		};
		Player player = new Player();
		Shop shop = new Shop(hand, new ShopRules(catalog), player);
		hand.TryAddCard(new CardData { Id = "book", Name = "黑暗魔法书", Cost = 5, BuildingType = "DarkMagicBook", ItemType = ShopItemTypes.DarkMagicBook });
		int goldBefore = shop.Gold;

		bool sold = shop.TrySellHandCard(0);

		Assert.True(sold);
		Assert.Empty(hand.Cards);
		Assert.Equal(goldBefore + 3, shop.Gold);
	}
}