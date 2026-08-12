// 魔晶矿井规则：产出间隔与判定
namespace CasualCastle.Domain.Building;

public static class CrystalMineRules
{
	public const string CrystalMine = "CrystalMine";
	public const float MiningIntervalSeconds = 20f;

	public static bool IsCrystalMine(string typeId)
	{
		return typeId == CrystalMine;
	}
}