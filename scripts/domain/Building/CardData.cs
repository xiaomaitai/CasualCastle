// 卡牌数据
using CasualCastle.Domain.Shared;

namespace CasualCastle.Domain.Building;

public class CardData
{
	public string Id { get; set; } = "";
	public string Name { get; set; } = "";
	public int Cost { get; set; }
	public string BuildingType { get; set; } = BuildingTypeIds.Barracks;
	public int Weight { get; set; } = 1;
	public string FactionId { get; set; } = "";

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
		};
	}
}
