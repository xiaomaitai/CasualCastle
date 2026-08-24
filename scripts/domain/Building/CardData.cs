// 卡牌数据
using CasualCastle.Domain.Shared;
using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public class CardData
{
	public string Id { get; set; } = "";
	public string Name { get; set; } = "";
	public int Cost { get; set; }
	public string BuildingType { get; set; } = BuildingTypeIds.Barracks;
	public int Weight { get; set; } = 1;
	public string FactionId { get; set; } = "";
	public string ItemType { get; set; } = "";
	public string UnlockRitualId { get; set; } = "";
	public int UnlockNight { get; set; } = 1;
	public Dictionary<string, string> Attributes { get; set; } = new();
	public bool IsPlaceable => string.IsNullOrEmpty(ItemType);

	public CardData Clone()
	{
		return new CardData
		{
			Id = Id,
			Name = Name,
			Cost = Cost,
			BuildingType = BuildingType,
			Weight = Weight,
			FactionId = FactionId,
			ItemType = ItemType,
			UnlockRitualId = UnlockRitualId,
			UnlockNight = UnlockNight,
			Attributes = new Dictionary<string, string>(Attributes),
		};
	}
}
