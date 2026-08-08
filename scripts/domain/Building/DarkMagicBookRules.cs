// 黑暗魔法书规则：物品卡与所记载仪式的映射
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

	public static string GetRitualId(string bookCardId)
	{
		if (bookCardId == null || !RitualIdByBookCardId.TryGetValue(bookCardId, out string ritualId))
			return null;
		return ritualId;
	}

	public static bool IsDarkMagicBook(string bookCardId)
	{
		return GetRitualId(bookCardId) != null;
	}
}
