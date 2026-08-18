// 法力生产组件：按规则与倍率每秒产出法力
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;

public sealed class ManaComponent : IBuildingComponent
{
	private readonly Building _owner;

	public ManaComponent(Building owner)
	{
		_owner = owner;
	}

	public void Tick(float delta)
	{
		if (!_owner.IsPlayerOwned || !ManaBuildingRules.IsManaProducer(_owner.TypeId) || !_owner.CanWork)
			return;
		ManaService mana = GameManager.Get<ManaService>();
		if (mana == null)
			return;
		mana.Add(ManaBuildingRules.GetManaPerSecond(_owner.TypeId) * DungeonFactionBuffs.ManaProductionMultiplier * delta);
	}
}
