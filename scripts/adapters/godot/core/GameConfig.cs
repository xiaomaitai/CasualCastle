using CasualCastle.Domain.Shared;

public static class GameConfig
{
	public const int DesignWidth = 1920;
	public const int DesignHeight = 1080;
	public const float DayDurationSeconds = GameRules.DayDurationSeconds;
	public const float NightDurationSeconds = GameRules.NightDurationSeconds;
	public const int InitialGold = GameRules.InitialGold;
	public const int RepairGoldPerHealth = GameRules.RepairGoldPerHealth;
	public const int CastleHeartMaxHealth = GameRules.CastleHeartMaxHealth;
}
