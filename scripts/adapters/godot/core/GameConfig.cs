using CasualCastle.Domain.Core;
using Godot;

public static class GameConfig
{
	// Display resolutions — Godot adapter concern, stays here
	public static readonly Vector2I[] OutputResolutions =
	{
		new(1920, 1080),
		new(1600, 900),
		new(1366, 768),
		new(1280, 720),
	};

	// Domain constants — delegate to GameRules
	public const int DesignWidth = GameRules.DesignWidth;
	public const int DesignHeight = GameRules.DesignHeight;
	public const float DayDurationSeconds = GameRules.DayDurationSeconds;
	public const float NightDurationSeconds = GameRules.NightDurationSeconds;
	public const int InitialGold = GameRules.InitialGold;
	public const int RepairGoldPerHealth = GameRules.RepairGoldPerHealth;
	public const int CastleHeartMaxHealth = GameRules.CastleHeartMaxHealth;
}
