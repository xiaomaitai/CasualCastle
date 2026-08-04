// 延迟调度适配器：用场景树一次性计时器实现领域延迟端口
using System;
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using Godot;

namespace CasualCastle.Adapters.Godot.Battle;

public class DelaySchedulerAdapter : IDelayScheduler
{
	public void Schedule(float delaySeconds, Action callback)
	{
		GameManager gameManager = AdapterRegistry.Resolve<GameManager>();
		SceneTreeTimer timer = gameManager.GetTree().CreateTimer(delaySeconds, processAlways: false);
		timer.Timeout += () => callback();
	}
}
