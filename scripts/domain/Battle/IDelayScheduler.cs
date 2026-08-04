// 领域延迟调度端口：提供一次性定时回调，避免按帧轮询
using System;

namespace CasualCastle.Domain.Battle;

public interface IDelayScheduler
{
	void Schedule(float delaySeconds, Action callback);
}
