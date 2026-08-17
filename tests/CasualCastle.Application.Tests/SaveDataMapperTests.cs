using CasualCastle.Application;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using System.Collections.Generic;
using Xunit;

namespace CasualCastle.Application.Tests;

public class SaveDataMapperTests
{
	[Fact]
	public void ToCardEntry_CopiesRuntimeAttributes()
	{
		CardData card = new()
		{
			Id = "tome",
			Name = "魔法书",
			Cost = 5,
			Weight = 2,
			ItemType = "dark_book",
			Attributes = new Dictionary<string, string> { { "charge", "7" } },
		};

		CardSaveEntry entry = SaveDataMapper.ToCardEntry(card);

		Assert.False(entry.IsPlaceable);
		Assert.Equal("dark_book", entry.ItemType);
		Assert.Equal("7", entry.Attributes["charge"]);
	}

	[Fact]
	public void ToBuildingEntry_CopiesPositionAndHealth()
	{
		BuildingSaveEntry entry = SaveDataMapper.ToBuildingEntry(new FakeBuildingState("barracks", 3, 4));

		Assert.Equal("barracks", entry.TypeId);
		Assert.Equal(3, entry.AnchorGridX);
		Assert.Equal(4, entry.AnchorGridY);
		Assert.Equal(100, entry.Health);
	}
}
