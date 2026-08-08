namespace CasualCastle.Domain.Shared;

public static class GameRules
{
	public const float DayDurationSeconds = 60f;
	public const float NightFactionDayDurationSeconds = 1f;
	public const float CombineDurationSeconds = 1f;
	public const float DayStartTimerHoldSeconds = 1f;
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

	public const float HordeAdjacencySpawnSpeedBonusPerCell = 0.05f;
	public const float HordeBurrowSpawnSpeedBonusPerBurrow = 0.01f;
	public const int HordeAdjacencyMaxCells = 4;
	public const float AuraRangeRadius = 150f;
	public const float WildCovenantExecuteThreshold = 0.30f;
	public const float WildHeartRegenDelaySeconds = 3f;
	public const float WildHeartRegenPercentPerSecond = 0.02f;

	public static readonly float[] DodgeChanceByAllyRaceCount = { 0f, 0f, 0.10f, 0.20f, 0.30f, 0.35f, 0.40f, 0.40f };
}
