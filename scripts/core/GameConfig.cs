using Godot;

public static class GameConfig
{
	public const int DesignWidth = 1920;
	public const int DesignHeight = 1080;
	public const int LegacyLayoutWidth = 1280;
	public const int LegacyLayoutHeight = 720;

	public static readonly Vector2 LayoutScale = new(
		(float)DesignWidth / LegacyLayoutWidth,
		(float)DesignHeight / LegacyLayoutHeight);

	public static readonly Vector2I[] OutputResolutions =
	{
		new(1920, 1080),
		new(1600, 900),
		new(1366, 768),
		new(1280, 720),
	};

	public const float DayDurationSeconds = 60f;
	public const float NightDurationSeconds = 30f;
	public const int InitialGold = 3000;
	public const int RepairGoldPerHealth = 1;
	public const int CastleHeartMaxHealth = 500;
}
