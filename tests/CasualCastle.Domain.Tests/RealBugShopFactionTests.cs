using System;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class RealBugShopFactionTests
{
	[Fact]
	public void Shop_SetFaction_AllianceDefaultPlayer_FiltersBothBuildingAndItemCards()
	{
		CardData[] catalog = new[]
		{
			new CardData { Id = "alliance_building", Name = "A", Cost = 3, BuildingType = "Barracks", Weight = 1, FactionId = FactionIds.Alliance },
			new CardData { Id = "horde_building", Name = "H", Cost = 3, BuildingType = "OrcCamp", Weight = 1, FactionId = FactionIds.Horde },
			new CardData { Id = "necromancer_item", Name = "I", Cost = 4, BuildingType = "", Weight = 1, FactionId = FactionIds.Necromancer, ItemType = ShopItemTypes.DarkMagicBook },
		};
		ShopRules rules = new ShopRules(catalog);
		Shop shop = new Shop(new Hand(new FakeBuildingPlacement(), new Player()), rules, new Player());
		shop.SetFaction(FactionIds.Alliance);

		CardData[] offers = rules.GenerateOffers(new Random(42));

		Assert.All(offers, card => Assert.Equal(FactionIds.Alliance, card.FactionId));
	}
}
