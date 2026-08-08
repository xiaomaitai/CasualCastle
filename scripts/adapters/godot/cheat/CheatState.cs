// 作弊开关状态：加速产兵等调试选项
using System;

public static class CheatState
{
	public static bool FastProductionEnabled { get; private set; }

	public static event Action FastProductionChanged;

	public static void SetFastProduction(bool enabled)
	{
		if (FastProductionEnabled == enabled)
			return;
		FastProductionEnabled = enabled;
		FastProductionChanged?.Invoke();
	}
}
