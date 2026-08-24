// 活动状态效果只读快照：种类与剩余秒数
namespace CasualCastle.Domain.Battle;

public sealed class ActiveStatusEffect
{
	public string Kind { get; }

	public float RemainingSeconds { get; }

	public ActiveStatusEffect(string kind, float remainingSeconds)
	{
		Kind = kind;
		RemainingSeconds = remainingSeconds;
	}
}
