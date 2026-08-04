namespace CasualCastle.Domain.Shared;

public static class GameRules
{
	public const float DayDurationSeconds = 60f;
	public const float CombineDurationSeconds = 3f;
	public const int InitialGold = 3000;
	public const int RepairGoldPerHealth = 1;
	public const int CastleHeartMaxHealth = 500;

	public const float SkillNearbyAllyRadius = 150f;
	public const float SkillTargetIsolatedRadius = 120f;

	public const float ProjectileSpeedMelee = 600f;
	public const float ProjectileSpeedRanged = 400f;
	public const float HealProjectileSpeed = 300f;
	public const float StealthSpeedMult = 0.85f;
	public const float GraceShieldMult = 1.5f;
	public const float GraceCooldownReduction = 1.0f;

	public const float HordeAdjacencySpawnSpeedBonusPerEmptyCell = 0.12f;
	public const int HordeAdjacencyMaxEmptyCells = 4;
	public const float AuraRangeRadius = 150f;
	public const float AuraAttackBonusMultiplier = 1.10f;
	public const float FuryLowHealthStepT1 = 0.20f;
	public const float FuryAttackBonusPerStepT1 = 0.10f;
	public const float FuryAttackMultiplierCapT1 = 1.40f;
	public const float FuryLowHealthStepT2 = 0.15f;
	public const float FuryAttackBonusPerStepT2 = 0.12f;
	public const float FuryAttackMultiplierCapT2 = 1.48f;
	public const float FuryAttackSpeedBonusPerStepT2 = 0.08f;
	public const float FuryAttackSpeedMultiplierCapT2 = 1.32f;

	public static readonly float[] DodgeChanceByAllyRaceCount = { 0f, 0f, 0.10f, 0.20f, 0.30f, 0.35f, 0.40f, 0.40f };
}
