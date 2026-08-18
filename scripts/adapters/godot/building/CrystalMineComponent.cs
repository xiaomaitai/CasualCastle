// 水晶矿组件：按固定间隔结算小水晶产出
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;

public sealed class CrystalMineComponent : IBuildingComponent
{
	private readonly Building _owner;

	private float _accumulator;

	public CrystalMineComponent(Building owner)
	{
		_owner = owner;
	}

	public void Tick(float delta)
	{
		if (!_owner.IsPlayerOwned || !CrystalMineRules.IsCrystalMine(_owner.TypeId) || !_owner.CanWork)
			return;
		_accumulator += delta;
		if (_accumulator < CrystalMineRules.MiningIntervalSeconds)
			return;
		_accumulator = 0f;
		GameManager.Get<KingExperienceService>()?.OnSmallCrystalMined();
	}
}
