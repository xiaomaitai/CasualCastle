// 音效策略：事件优先级（0-100）、同类声音分组与播放限流参数，供参数编辑器热修改
public static class SfxPolicy
{
	public const int MaxPlaysPerSecondDefault = 30;
	public const int GroupMinIntervalMsDefault = 100;
	public const int OverloadThresholdDefault = 50;
	public const int ThresholdDecayPerSecondDefault = 20;

	public static int MaxPlaysPerSecond = MaxPlaysPerSecondDefault;

	public static int GroupMinIntervalMs = GroupMinIntervalMsDefault;

	public static int OverloadThreshold = OverloadThresholdDefault;

	public static int ThresholdDecayPerSecond = ThresholdDecayPerSecondDefault;

	public static string GetGroup(string eventName)
	{
		switch (eventName)
		{
			case SfxEvents.ButtonClick:
			case SfxEvents.SliderChange:
				return "ui_click";
			case SfxEvents.UnitSelect:
			case SfxEvents.BuildingSelect:
				return "ui_select";
			case SfxEvents.ShopOpen:
			case SfxEvents.ShopClose:
				return "ui_shop";
			case SfxEvents.Buy:
			case SfxEvents.CoinChange:
			case SfxEvents.Place:
			case SfxEvents.PlaceCancel:
			case SfxEvents.Repair:
			case SfxEvents.Combine:
			case SfxEvents.Upgrade:
				return "ui_economy";
			case SfxEvents.MeleeAttack:
			case SfxEvents.MeleeSlash:
			case SfxEvents.MeleePierce:
				return "combat_attack";
			case SfxEvents.Hit:
			case SfxEvents.HitSlash:
			case SfxEvents.HitPierce:
			case SfxEvents.HitMagic:
				return "combat_hit";
			case SfxEvents.ProjectileLaunch:
			case SfxEvents.LaunchArrow:
			case SfxEvents.LaunchMagic:
			case SfxEvents.ProjectileHit:
				return "combat_ranged";
			case SfxEvents.UnitDeath:
				return "combat_death";
			case SfxEvents.BuildingHit:
			case SfxEvents.BuildingDestroy:
				return "combat_building";
			case SfxEvents.PhaseChange:
				return "phase";
			case SfxEvents.Victory:
			case SfxEvents.Defeat:
				return "result";
			default:
				return "misc";
		}
	}

	public static int GetPriority(string eventName)
	{
		switch (eventName)
		{
			case SfxEvents.Victory:
			case SfxEvents.Defeat:
				return 100;
			case SfxEvents.PhaseChange:
				return 90;
			case SfxEvents.UnitDeath:
				return 85;
			case SfxEvents.BuildingDestroy:
				return 80;
			case SfxEvents.Buy:
			case SfxEvents.Upgrade:
				return 70;
			case SfxEvents.Hit:
			case SfxEvents.HitSlash:
			case SfxEvents.HitPierce:
			case SfxEvents.HitMagic:
			case SfxEvents.BuildingHit:
				return 65;
			case SfxEvents.Repair:
			case SfxEvents.Combine:
			case SfxEvents.CoinChange:
				return 60;
			case SfxEvents.MeleeAttack:
			case SfxEvents.MeleeSlash:
			case SfxEvents.MeleePierce:
			case SfxEvents.ProjectileLaunch:
			case SfxEvents.LaunchArrow:
			case SfxEvents.LaunchMagic:
			case SfxEvents.ProjectileHit:
				return 55;
			case SfxEvents.Place:
			case SfxEvents.PlaceCancel:
				return 50;
			case SfxEvents.ShopOpen:
			case SfxEvents.ShopClose:
				return 45;
			case SfxEvents.UnitSelect:
			case SfxEvents.BuildingSelect:
				return 40;
			case SfxEvents.ButtonClick:
			case SfxEvents.SliderChange:
				return 30;
			default:
				return 50;
		}
	}
}
