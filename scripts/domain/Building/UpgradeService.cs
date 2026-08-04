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

	public IBuildingState TryUpgrade(IBuildingState source, string resultTypeId, float healthRatio, Func<int, bool> trySpendGold, IUpgradeBuildingFactory factory)
	{
		if (!_upgradeRules.CanUpgrade(source, _buildingRepo))
			return null;

		UpgradeRecipe recipe = _upgradeRules.FindRecipe(source.TypeId);
		if (recipe == null || recipe.ResultTypeId != resultTypeId)
			return null;

		int cost = _upgradeRules.CalculateCost(source.TypeId, resultTypeId, _buildingRepo);
		if (cost < 0 || !trySpendGold(cost))
			return null;

		IBuildingState result = factory.Create(resultTypeId, source.AnchorGridX, source.AnchorGridY, healthRatio);
		if (result == null)
			return null;

		factory.Destroy(source);
		return result;
	}
}
