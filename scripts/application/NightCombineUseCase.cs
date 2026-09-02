// 夜间敌方复刻用例：白天开始时应用战报快照
using CasualCastle.Domain.History;
using System;

namespace CasualCastle.Application;

public class NightCombineUseCase
{
	private readonly IReplayUseCase _replayService;

	public NightCombineUseCase(IReplayUseCase replayService)
	{
		_replayService = replayService;
	}

	public void ApplyReplaySnapshot(IReplayTarget target, int nightIndex, BattlePhase phase, Action onApplied)
	{
		if (target == null)
			return;
		_replayService.ApplyNightSnapshot(target, nightIndex, phase);
		onApplied();
	}
}
