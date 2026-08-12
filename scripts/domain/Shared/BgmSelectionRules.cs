// BGM 选择规则：舒缓曲目仅限第一天，NightIndex 从 1 起全部使用激昂曲目
using System.Collections.Generic;

namespace CasualCastle.Domain.Shared;

public static class BgmSelectionRules
{
	public const int FirstDayNightIndex = 0;

	private static readonly string[] DayFilteredPools =
	{
		BgmPoolIds.AllianceDay,
		BgmPoolIds.AllianceNight,
		BgmPoolIds.HordeDay,
		BgmPoolIds.HordeNight,
	};

	public static List<BgmDef> SelectTracks(List<BgmDef> poolTracks, string pool, int nightIndex)
	{
		if (!IsDayFilteredPool(pool))
			return poolTracks;

		BgmStyle style = nightIndex <= FirstDayNightIndex ? BgmStyle.Soothing : BgmStyle.Stirring;
		List<BgmDef> filtered = new();
		foreach (BgmDef def in poolTracks)
		{
			if (def.Style == style)
				filtered.Add(def);
		}

		return filtered.Count > 0 ? filtered : poolTracks;
	}

	private static bool IsDayFilteredPool(string pool)
	{
		foreach (string filteredPool in DayFilteredPools)
		{
			if (filteredPool == pool)
				return true;
		}

		return false;
	}
}