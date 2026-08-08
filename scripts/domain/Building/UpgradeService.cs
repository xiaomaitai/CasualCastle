// 升级服务：校验并执行建筑升级
using System;

namespace CasualCastle.Domain.Building;

public class UpgradeService : IUpgradeUseCase
{
	private readonly UpgradeRules _upgradeRules;
	private readonly IBuildingRepository _buildingRepo;

	public UpgradeService(UpgradeRules upgradeRules, IBuildingRepository buildingRepo)
	{
		_upgradeRules = upgradeRules;
		_buildingRepo = buildingRepo;
	}

	public IBuildingState TryUpgrade(IBuildingState source, string resultTypeId, float healthRatio, Func<int, bool> trySpendGold, IUpgradeBuildingFactory factory, int coreLevel = int.MaxValue, int burrowCount = 0)
	{
		if (!_upgradeRules.CanUpgrade(source, _buildingRepo, coreLevel, burrowCount))
			return null;

		UpgradeRecipe recipe = _upgradeRules.FindRecipe(source.TypeId);
		if (recipe == null || recipe.ResultTypeId != resultTypeId)
			return null;

		int cost = _upgradeRules.CalculateCost(source.TypeId, resultTypeId, _buildingRepo);
		if (cost < 0 || !trySpendGold(cost))
			return null;

		factory.Destroy(source);
		IBuildingState result = factory.Create(resultTypeId, source.AnchorGridX, source.AnchorGridY, healthRatio);
		return result;
	}
}
