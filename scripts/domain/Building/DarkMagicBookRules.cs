// 黑暗魔法书规则：魔法书物品卡与其记载仪式的映射
using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public static class DarkMagicBookRules
{
	private static readonly Dictionary<string, string> RitualIdByBookCardId = new();

	public static void Load(IReadOnlyList<KeyValuePair<string, string>> entries)
	{
		RitualIdByBookCardId.Clear();
		foreach (KeyValuePair<string, string> entry in entries)
			RitualIdByBookCardId[entry.Key] = entry.Value;
	}

	public static string GetRitualId(CardData card)
	{
		if (card == null || card.ItemType != ShopItemTypes.DarkMagicBook)
			return null;
		return GetRitualId(card.Id);
	}

	public static bool IsDarkMagicBook(CardData card)
	{
		return GetRitualId(card) != null;
	}

	private static string GetRitualId(string bookCardId)
	{
		if (bookCardId == null || !RitualIdByBookCardId.TryGetValue(bookCardId, out string ritualId))
			return null;
		return ritualId;
	}
}