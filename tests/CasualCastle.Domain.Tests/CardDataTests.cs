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
			Weight = 3
		};
		CardData clone = original.Clone();

		Assert.Equal(original.Id, clone.Id);
		Assert.Equal(original.Name, clone.Name);
		Assert.Equal(original.Cost, clone.Cost);
		Assert.Equal(original.BuildingType, clone.BuildingType);
		Assert.Equal(original.Weight, clone.Weight);
		Assert.NotSame(original, clone);
	}
}
