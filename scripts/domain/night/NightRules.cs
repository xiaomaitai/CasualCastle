namespace CasualCastle.Domain.Night;

public static class NightRules
{
	public static bool CanUnitWork(bool hasNightCombat, bool isDay)
	{
		return isDay || hasNightCombat;
	}
}
