// 魔王规则：种族判定、无敌与舒适度/经验常量
namespace CasualCastle.Domain.Battle;

public static class DemonKingRules
{
	public const string DemonKingRace = "DemonKing";
	public const string DemonRace = "Demon";
	public const float MaxComfort = 100f;
	public const float CombatResumeThreshold = 25f;
	public const float AttackComfortCost = 5f;
	public const float HitComfortCost = 4f;
	public const float DistanceComfortCostPerSecond = 2f;
	public const float RestorePerSecondNearThrone = 8f;
	public const float RestorePerSecondIdle = 3f;
	public const float ThroneRestoreRadius = 80f;
	public const int FriendlyDeathXp = 5;
	public const int EnemyDeathXp = 10;
	public const int KingKillXp = 15;
	public const int CrystalXp = 50;
	public const int SmallCrystalXp = 15;
	public const int BossDropCostThreshold = 5000;
	public static readonly int[] LevelThresholds = { 100, 250, 500, 1000 };
	public static readonly float[] PatrolRadiusByLevel = { 120f, 170f, 220f, 270f, 320f };

	public static bool IsDemonKing(string race)
	{
		return race == DemonKingRace;
	}

	public static bool IsDemonRace(string race)
	{
		return race == DemonKingRace || race == DemonRace;
	}

	public static float GetPatrolRadius(int level)
	{
		if (level < 1 || level > PatrolRadiusByLevel.Length)
			return PatrolRadiusByLevel[0];
		return PatrolRadiusByLevel[level - 1];
	}

	public static int GetRequiredExperienceForLevel(int level)
	{
		if (level < 1 || level > LevelThresholds.Length)
			return int.MaxValue;
		return LevelThresholds[level - 1];
	}
}