// 陷阱组件：冷却计时、自瞄传送触发与陷阱源创建
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using Godot;

public sealed class TrapComponent : IBuildingComponent
{
	private readonly Building _owner;

	private float _cooldown;

	private CasualCastle.Domain.Battle.Unit _trapSource;

	public TrapComponent(Building owner)
	{
		_owner = owner;
	}

	public void Tick(float delta)
	{
		if (!TrapRules.IsTrap(_owner.TypeId) || !_owner.CanWork)
			return;
		if (_cooldown > 0f)
		{
			_cooldown -= delta;
			return;
		}
		TrapService trapService = GameManager.Get<TrapService>();
		if (trapService == null)
			return;

		_trapSource ??= TrapSourceFactory.Create(_owner.IsPlayerOwned);
		float centerX = GameCoordinatesAdapter.PixelsToGameUnits(_owner.GlobalPosition.X);
		float centerY = GameCoordinatesAdapter.PixelsToGameUnits(_owner.GlobalPosition.Y);
		(float teleportX, float teleportY) = GetTeleportTarget();
		int hits = trapService.Trigger(_trapSource, _owner.TypeId, centerX, centerY, teleportX, teleportY);
		if (hits > 0)
			_cooldown = TrapRules.GetConfig(_owner.TypeId).Cooldown * DungeonFactionBuffs.TrapCooldownMultiplier;
	}

	private (float x, float y) GetTeleportTarget()
	{
		GameManager gameManager = AdapterRegistry.Resolve<GameManager>();
		Castle enemyCastle = _owner.IsPlayerOwned ? gameManager?.EnemyCastle : gameManager?.PlayerCastle;
		if (enemyCastle?.Heart != null)
		{
			Vector2 position = enemyCastle.Heart.GlobalPosition;
			return (GameCoordinatesAdapter.PixelsToGameUnits(position.X), GameCoordinatesAdapter.PixelsToGameUnits(position.Y));
		}
		return (GameCoordinatesAdapter.PixelsToGameUnits(_owner.GlobalPosition.X), GameCoordinatesAdapter.PixelsToGameUnits(_owner.GlobalPosition.Y));
	}
}
