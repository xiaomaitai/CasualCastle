using CasualCastle.Domain.Building;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class CardDataTests
{
	[Fact]
	public void Clone_ReturnsNewInstanceWithSameValues()
	{
		CardData original = new()
		{
			Id = "card_1",
			Name = "Test Card",
			Cost = 50,
			BuildingType = "Barracks",
			Weight = 3,
			ItemType = ShopItemTypes.DarkMagicBook
		};
		CardData clone = original.Clone();

		Assert.Equal(original.Id, clone.Id);
		Assert.Equal(original.Name, clone.Name);
		Assert.Equal(original.Cost, clone.Cost);
		Assert.Equal(original.BuildingType, clone.BuildingType);
		Assert.Equal(original.Weight, clone.Weight);
		Assert.Equal(original.ItemType, clone.ItemType);
		Assert.False(clone.IsPlaceable);
		Assert.NotSame(original, clone);
	}
	[Fact]
	public void IsPlaceable_BuildingCard_ReturnsTrue()
	{
		CardData card = new() { Id = "tomb", BuildingType = "Tomb" };

		Assert.True(card.IsPlaceable);
	}

	[Fact]
	public void IsPlaceable_ItemCard_ReturnsFalse()
	{
		CardData card = new() { Id = "book", ItemType = ShopItemTypes.DarkMagicBook };

		Assert.False(card.IsPlaceable);
	}
}
