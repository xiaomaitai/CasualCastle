// 瘟疫扩散组件：对范围内敌人持续施加伤害
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;

public sealed class PlagueComponent : IBuildingComponent
{
	private readonly Building _owner;

	private float _accumulator;

	private CasualCastle.Domain.Battle.Unit _trapSource;

	public PlagueComponent(Building owner)
	{
		_owner = owner;
	}

	public void Tick(float delta)
	{
		if (DungeonFactionBuffs.PlagueSpreadPerSecond <= 0f || !_owner.CanWork)
			return;
		if (AdapterRegistry.Resolve<BuildingSystem>().GetFactionId(_owner.TypeId) != FactionDef.DungeonId)
			return;
		_accumulator += DungeonFactionBuffs.PlagueSpreadPerSecond * delta;
		int damage = (int)_accumulator;
		if (damage <= 0)
			return;
		_accumulator -= damage;
		ITacticalQueries tactical = GameManager.Get<ITacticalQueries>();
		if (tactical == null)
			return;
		_trapSource ??= TrapSourceFactory.Create(_owner.IsPlayerOwned);
		float centerX = GameCoordinatesAdapter.PixelsToGameUnits(_owner.GlobalPosition.X);
		float centerY = GameCoordinatesAdapter.PixelsToGameUnits(_owner.GlobalPosition.Y);
		foreach (IUnitHandle enemy in tactical.FindEnemiesNearPoint(_trapSource, centerX, centerY, DungeonFactionBuffs.PlagueSpreadRange))
			enemy.TakeDamage(damage, null, centerX, centerY, isDot: true);
	}
}
