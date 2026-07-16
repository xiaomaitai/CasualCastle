namespace CasualCastle.Domain.Shared;

public static class GameRules
{
	public const float DayDurationSeconds = 60f;
	public const float NightDurationSeconds = 30f;
	public const int InitialGold = 3000;
	public const int RepairGoldPerHealth = 1;
	public const int CastleHeartMaxHealth = 500;

	public const float SkillNearbyAllyRadius = 150f;
	public const float SkillTargetIsolatedRadius = 120f;

	public const float ProjectileSpeedMelee = 600f;
	public const float ProjectileSpeedRanged = 400f;

	public static readonly float[] DodgeChanceByAllyRaceCount = { 0f, 0f, 0.10f, 0.20f, 0.30f, 0.35f, 0.40f, 0.40f };
}
