// 存档映射器：运行时状态与存档条目互转，含旧档 catalog 回填
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using System.Collections.Generic;

namespace CasualCastle.Application;

public static class SaveDataMapper
{
	public static BuildingSaveEntry ToBuildingEntry(IBuildingState building)
	{
		return new BuildingSaveEntry
		{
			TypeId = building.TypeId,
			AnchorGridX = building.AnchorGridX,
			AnchorGridY = building.AnchorGridY,
			Health = building.Health,
		};
	}

	public static CardSaveEntry ToCardEntry(CardData card)
	{
		return new CardSaveEntry
		{
			Id = card.Id,
			Name = card.Name,
			Cost = card.Cost,
			BuildingType = card.BuildingType,
			Weight = card.Weight,
			IsPlaceable = card.IsPlaceable,
			ItemType = card.ItemType,
			Attributes = new Dictionary<string, string>(card.Attributes),
		};
	}

	public static CardData ToCardData(CardSaveEntry card, IReadOnlyList<CardData> catalog)
	{
		CardData restored = new()
		{
			Id = card.Id,
			Name = card.Name,
			Cost = card.Cost,
			BuildingType = card.BuildingType,
			Weight = card.Weight,
			ItemType = card.ItemType,
			Attributes = new Dictionary<string, string>(card.Attributes),
		};
		if (string.IsNullOrEmpty(restored.ItemType) && !card.IsPlaceable)
			CopyItemFromCatalog(restored, catalog);
		return restored;
	}

	private static void CopyItemFromCatalog(CardData restored, IReadOnlyList<CardData> catalog)
	{
		foreach (CardData catalogCard in catalog)
		{
			if (catalogCard.Id == restored.Id)
			{
				restored.ItemType = catalogCard.ItemType;
				restored.Attributes = new Dictionary<string, string>(catalogCard.Attributes);
				return;
			}
		}
	}
}
