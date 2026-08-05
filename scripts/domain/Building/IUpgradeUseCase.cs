using System;

namespace CasualCastle.Domain.Building;

public interface IUpgradeUseCase
{
	IBuildingState TryUpgrade(IBuildingState source, string resultTypeId, float healthRatio, Func<int, bool> trySpendGold, IUpgradeBuildingFactory factory, int coreLevel = int.MaxValue, int burrowCount = 0);
}
