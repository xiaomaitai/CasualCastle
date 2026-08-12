// 奸奇魔力建筑规则：魔力产出建筑与速率
namespace CasualCastle.Domain.Building;

public static class ManaBuildingRules
{
	public const string ManaSpring = "ManaSpring";
	public const string RuneCircle = "RuneCircle";
	public const string AbyssEye = "AbyssEye";

	public static bool IsManaProducer(string typeId)
	{
		return typeId == ManaSpring || typeId == RuneCircle || typeId == AbyssEye;
	}

	public static float GetManaPerSecond(string typeId)
	{
		switch (typeId)
		{
			case ManaSpring:
				return 4f;
			case RuneCircle:
				return 6f;
			case AbyssEye:
				return 8f;
			default:
				return 0f;
		}
	}
}