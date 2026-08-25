// 作弊开关状态：加速产兵、显示碰撞体积等调试选项
using System;

public static class CheatState
{
	public static bool FastProductionEnabled { get; private set; }

	public static bool ShowCollisionEnabled { get; private set; }

	public static event Action FastProductionChanged;

	public static event Action ShowCollisionChanged;

	public static void SetFastProduction(bool enabled)
	{
		if (FastProductionEnabled == enabled)
			return;
		FastProductionEnabled = enabled;
		FastProductionChanged?.Invoke();
	}

	public static void SetShowCollision(bool enabled)
	{
		if (ShowCollisionEnabled == enabled)
			return;
		ShowCollisionEnabled = enabled;
		ShowCollisionChanged?.Invoke();
	}

	public static void ResetAll()
	{
		SetFastProduction(false);
		SetShowCollision(false);
	}
}
