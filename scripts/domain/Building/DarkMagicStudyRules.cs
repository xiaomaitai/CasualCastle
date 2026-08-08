// 黑魔法研习规则：随完成次数递增的黑魔法消耗
namespace CasualCastle.Domain.Building;

public static class DarkMagicStudyRules
{
	public const int BaseCost = 20;
	public const int CostIncrementPerCompletion = 20;
	public const float DurationSeconds = 10f;

	public static int GetCost(int completionCount)
	{
		return BaseCost + CostIncrementPerCompletion * completionCount;
	}
}
