// 夜间结算用例：合成结算与敌方复刻应用
using CasualCastle.Domain.Building;
using CasualCastle.Domain.History;
using System;
using System.Collections.Generic;

namespace CasualCastle.Application;

public class NightCombineUseCase
{
	private readonly IBuildingRepository _buildingRepository;
	private readonly IReplayUseCase _replayService;
	private readonly ICombineUseCase _combineService;

	public NightCombineUseCase(IBuildingRepository buildingRepository, IReplayUseCase replayService, ICombineUseCase combineService)
	{
		_buildingRepository = buildingRepository;
		_replayService = replayService;
		_combineService = combineService;
	}

	public void ResolveNightCombines(List<IBuildingState> buildings, string factionId, bool isCombine, bool isPlaying, ICombineBuildingFactory factory, Action onCombined)
	{
		int coreLevel = ComputeCoreLevel(factionId, buildings);
		_combineService.ResolveCombines(buildings, true, isCombine, isPlaying, factory, _ => onCombined(), coreLevel);
	}

	public void ApplyReplaySnapshot(IReplayTarget target, int nightIndex, BattlePhase phase, Action onApplied)
	{
		if (target == null)
			return;
		_replayService.ApplyNightSnapshot(target, nightIndex, phase);
		onApplied();
	}

	private int ComputeCoreLevel(string factionId, List<IBuildingState> buildings)
	{
		if (factionId == FactionDef.NecromancerId)
			return RitualCircleRules.GetCoreLevel(buildings);
		if (factionId == FactionDef.DungeonId)
			return DungeonCoreRules.GetCoreLevel(buildings);
		return int.MaxValue;
	}
}
