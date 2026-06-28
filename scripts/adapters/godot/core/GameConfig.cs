using CasualCastle.Domain.Shared;

// Thin facade over domain GameRules for backward compatibility.
// Display-specific config (OutputResolutions) moved to DisplaySettingsManager.
public static class GameConfig
{
	public const int DesignWidth = GameRules.DesignWidth;
	public const int DesignHeight = GameRules.DesignHeight;
	public const float DayDurationSeconds = GameRules.DayDurationSeconds;
	public const float NightDurationSeconds = GameRules.NightDurationSeconds;
	public const int InitialGold = GameRules.InitialGold;
	public const int RepairGoldPerHealth = GameRules.RepairGoldPerHealth;
	public const int CastleHeartMaxHealth = GameRules.CastleHeartMaxHealth;
}
