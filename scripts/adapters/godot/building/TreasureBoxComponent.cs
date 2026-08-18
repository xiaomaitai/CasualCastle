// 宝箱组件：吸引敌人、引燃爆炸与冷却
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;

public sealed class TreasureBoxComponent : IBuildingComponent
{
	private readonly Building _owner;

	private float _fuse;

	private float _cooldown;

	private CasualCastle.Domain.Battle.Unit _trapSource;

	public TreasureBoxComponent(Building owner)
	{
		_owner = owner;
	}

	public void Tick(float delta)
	{
		if (_owner.TypeId != TrapRules.TreasureBox || !_owner.CanWork)
			return;
		TrapService trapService = GameManager.Get<TrapService>();
		if (trapService == null)
			return;
		if (_cooldown > 0f)
		{
			_cooldown -= delta;
			return;
		}

		ITacticalQueries tactical = GameManager.Get<ITacticalQueries>();
		if (tactical == null)
			return;
		_trapSource ??= TrapSourceFactory.Create(_owner.IsPlayerOwned);
		float centerX = GameCoordinatesAdapter.PixelsToGameUnits(_owner.GlobalPosition.X);
		float centerY = GameCoordinatesAdapter.PixelsToGameUnits(_owner.GlobalPosition.Y);
		TrapConfig config = TrapRules.GetConfig(_owner.TypeId);
		foreach (IUnitHandle enemy in tactical.FindEnemiesNearPoint(_trapSource, centerX, centerY, config.AttractRange))
			trapService.PullTowards(enemy, centerX, centerY, config.PullSpeed, delta);
		if (tactical.FindEnemiesNearPoint(_trapSource, centerX, centerY, config.AttractRange).Count == 0)
			return;
		_fuse += delta;
		if (_fuse < config.FuseSeconds)
			return;
		_fuse = 0f;
		trapService.Trigger(_trapSource, _owner.TypeId, centerX, centerY, 0f, 0f);
		_cooldown = config.Cooldown * DungeonFactionBuffs.TrapCooldownMultiplier;
	}
}
