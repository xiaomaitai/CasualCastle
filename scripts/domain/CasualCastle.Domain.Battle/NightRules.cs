namespace CasualCastle.Domain.Battle;

public static class NightRules
{
	public static bool CanUnitWork(bool hasNightCombat, bool isDay)
	{
		return isDay || hasNightCombat;
	}
}
