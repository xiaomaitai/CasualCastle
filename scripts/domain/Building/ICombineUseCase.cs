// 组合用例端口
using System;
using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public interface ICombineUseCase
{
	void ResolveCombines(List<IBuildingState> buildings, bool isPlayerSide, bool isCombinePhase, bool isPlaying, ICombineBuildingFactory factory, Action<IBuildingState> onCombineCompleted, int coreLevel = int.MaxValue);
}
