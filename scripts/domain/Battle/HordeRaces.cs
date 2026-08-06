// 部落五族标识辅助：判定单位是否属于部落种族
using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public static class HordeRaces
{
	private static readonly HashSet<string> RaceNames = new() { "Orc", "Goblin", "Centaur", "Staglur", "Lizard" };

	public static bool IsHordeRace(string race)
	{
		return !string.IsNullOrEmpty(race) && RaceNames.Contains(race);
	}
}
