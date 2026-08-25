// 卡牌悬停提示：物品卡显示道具说明，建筑卡显示产出单位属性
using CasualCastle.Domain.Building;

public static class CardTooltipBuilder
{
	private const string DungeonCrystalTooltip = "经验魔晶：立即为魔王注入经验";

	public static bool IsItemCard(CardData card)
	{
		return !string.IsNullOrEmpty(card.ItemType);
	}

	public static string BuildItemTooltip(CardData card)
	{
		if (card.ItemType == ShopItemTypes.DungeonCrystal)
			return DungeonCrystalTooltip;
		if (card.ItemType == ShopItemTypes.DarkMagicBook)
			return $"魔法书：{GetRitualDisplayName(card)}";
		return $"{card.Name}：道具";
	}

	private static string GetRitualDisplayName(CardData card)
	{
		string ritualId = DarkMagicBookRules.GetRitualId(card);
		if (ritualId == null)
			return "研习仪式";
		return RitualRules.Get(ritualId).DisplayName;
	}
}