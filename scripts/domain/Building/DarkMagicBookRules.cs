// 黑暗魔法书规则：按物品类型与属性解析所记载的仪式
namespace CasualCastle.Domain.Building;

public static class DarkMagicBookRules
{
	public const string RitualAttributeKey = "ritual_id";

	public static string GetRitualId(CardData card)
	{
		if (card == null || card.ItemType != ShopItemTypes.DarkMagicBook)
			return null;
		return card.Attributes.TryGetValue(RitualAttributeKey, out string ritualId) ? ritualId : null;
	}

	public static bool IsDarkMagicBook(CardData card)
	{
		return GetRitualId(card) != null;
	}
}