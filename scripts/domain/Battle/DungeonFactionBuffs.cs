// 地下城全局增益：T5 建筑与终极技能效果开关
namespace CasualCastle.Domain.Battle;

public static class DungeonFactionBuffs
{
	public const float ActivePatrolRadius = 600f;
	public const float ComfortParadiseBonus = 50f;
	public const float TrapCooldownHalvedMultiplier = 0.5f;
	public const float KillXpBloodThroneMultiplier = 1.5f;
	public const float GlobalRegenFractionPerSecond = 0.01f;
	public const float PlagueSpreadPerSecondValue = 4f;
	public const float PlagueSpreadRange = 90f;
	public const float FastBreedProductionMultiplier = 1.3f;
	public const float ManaProductionLabyrinthMultiplier = 1.5f;
	public const float AuraRangeSkillMultiplier = 1.5f;
	public const float PleasureStageAttackMultiplier = 1.1f;

	public static bool ActivePatrolEnabled;
	public static float TrapCooldownMultiplier = 1f;
	public static float ComfortMaxBonus;
	public static float KillXpMultiplier = 1f;
	public static float GlobalRegenPerSecond;
	public static bool AutoBreedEnabled;
	public static float ProductionSpeedMultiplier = 1f;
	public static float PlagueSpreadPerSecond;
	public static float ManaProductionMultiplier = 1f;
	public static float AuraRangeMultiplier = 1f;
	public static float LivingRegenPerSecond;
	public static bool TrapCapDoubled;
	public static float AttackDamageMultiplier = 1f;

	public static void Reset()
	{
		ActivePatrolEnabled = false;
		TrapCooldownMultiplier = 1f;
		ComfortMaxBonus = 0f;
		KillXpMultiplier = 1f;
		GlobalRegenPerSecond = 0f;
		AutoBreedEnabled = false;
		ProductionSpeedMultiplier = 1f;
		PlagueSpreadPerSecond = 0f;
		ManaProductionMultiplier = 1f;
		AuraRangeMultiplier = 1f;
		LivingRegenPerSecond = 0f;
		TrapCapDoubled = false;
		AttackDamageMultiplier = 1f;
	}
}