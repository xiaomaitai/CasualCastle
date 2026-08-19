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

	[Fact]
	public void ToCardData_RestoresPlaceableCardFields()
	{
		CardSaveEntry entry = new()
		{
			Id = "barracks",
			Name = "兵营",
			Cost = 3,
			BuildingType = "Barracks",
			Weight = 1,
			IsPlaceable = true,
			Attributes = new Dictionary<string, string> { { "capacity", "4" } },
		};

		CardData restored = SaveDataMapper.ToCardData(entry, new List<CardData>());

		Assert.Equal("barracks", restored.Id);
		Assert.Equal(3, restored.Cost);
		Assert.Equal("Barracks", restored.BuildingType);
		Assert.Equal("4", restored.Attributes["capacity"]);
	}

	[Fact]
	public void ToCardData_OldSaveWithoutItemType_BackfillsFromCatalog()
	{
		CardSaveEntry oldEntry = new()
		{
			Id = "tome",
			Name = "魔法书",
			Cost = 5,
			Weight = 2,
			IsPlaceable = false,
		};
		CardData catalogCard = new()
		{
			Id = "tome",
			Name = "魔法书",
			Cost = 5,
			Weight = 2,
			ItemType = "dark_book",
			Attributes = new Dictionary<string, string> { { "charge", "7" } },
		};

		CardData restored = SaveDataMapper.ToCardData(oldEntry, new List<CardData> { catalogCard });

		Assert.Equal("dark_book", restored.ItemType);
		Assert.Equal("7", restored.Attributes["charge"]);
	}
}
