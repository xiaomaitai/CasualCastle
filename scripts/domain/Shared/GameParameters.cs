// 运行时全局参数：全局常量的可热修改来源，供开发者参数编辑器读写
namespace CasualCastle.Domain.Shared;

public static class GameParameters
{
	public static int InitialGold = GameRules.InitialGold;
	public static float CombatDurationSeconds = GameRules.CombatDurationSeconds;
	public static float CombineDurationSeconds = GameRules.CombineDurationSeconds;
	public static float CombatStartTimerHoldSeconds = GameRules.CombatStartTimerHoldSeconds;
	public static int RepairGoldPerHealth = GameRules.RepairGoldPerHealth;

	public static void Reset()
	{
		InitialGold = GameRules.InitialGold;
		CombatDurationSeconds = GameRules.CombatDurationSeconds;
		CombineDurationSeconds = GameRules.CombineDurationSeconds;
		CombatStartTimerHoldSeconds = GameRules.CombatStartTimerHoldSeconds;
		RepairGoldPerHealth = GameRules.RepairGoldPerHealth;
	}
}
