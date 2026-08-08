// 单位逻辑战斗：物理帧驱动、索敌移动与攻击表现
using System.Collections.Generic;
using CasualCastle.Adapters.Godot;
using CasualCastle.Adapters.Godot.Battle;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Godot;

public partial class UnitLogic
{
	private const float MinMoveDirectionLength = 0.001f;
	private const float FlipThresholdSpeed = 0.1f;
	private const float SmallDisplaySize = 60f;
	private const float MediumDisplaySize = 90f;
	private const float LargeDisplaySize = 120f;
	private const float HugeDisplaySize = 180f;
	public override void _PhysicsProcess(double delta)
	{
		if (!IsAlive) return;
		if (AdapterRegistry.Resolve<GameManager>()?.CurrentState == GameManager.GameState.GameOver) return;

		float dt = (float)delta;

		_visual?.UpdateHitFlash(dt);

		Vector2 pushedPixelPos = new Vector2(
			GameCoordinatesAdapter.GameUnitsToPixels(_unit.GameX),
			GameCoordinatesAdapter.GameUnitsToPixels(_unit.GameY));
		if ((pushedPixelPos - _body.GlobalPosition).LengthSquared() > PushSyncEpsilonSq)
			_body.GlobalPosition = pushedPixelPos;

		UpdateSleepVisual();
		UpdateStealthVisual();
		if (!IsActive) return;
		if (_fieldRepo == null || _body == null) return;

		(IUnitHandle nearest, _) = _tactical.FindNearestEnemy(_unit);

		IBuildingTarget buildingTarget = _tactical.FindOverlappingBuilding(_unit);
		if (buildingTarget != null)
			_unit.SetBuildingTarget(buildingTarget);
		else
			_unit.ClearBuildingTarget();

		_unit.UpdateTargeting(nearest);

		float marchX, marchY;
		Vector2 marchDest = SelectTarget();
		marchX = GameCoordinatesAdapter.PixelsToGameUnits(marchDest.X);
		marchY = GameCoordinatesAdapter.PixelsToGameUnits(marchDest.Y);

		_unit.UpdateBehavior(dt, marchX, marchY);

		Vector2 currentPixelPos = _body.GlobalPosition;
		Vector2 nextPathPos = _navigationAgent.GetNextPathPosition();
		Vector2 direction = nextPathPos - currentPixelPos;
		float pixelSpeed = GameCoordinatesAdapter.GameUnitsToPixels(_unit.GetCurrentSpeed());
		Vector2 desiredVelocity = direction.Length() > MinMoveDirectionLength
			? direction.Normalized() * pixelSpeed
			: Vector2.Zero;
		_navigationAgent.Velocity = desiredVelocity;

		Vector2 moveVelocity = _safeVelocity != Vector2.Zero ? _safeVelocity : desiredVelocity;
		_body.GlobalPosition += moveVelocity * dt;

		if (Mathf.Abs(moveVelocity.X) > FlipThresholdSpeed)
			_visual.SetFlipH(moveVelocity.X < 0);

		_unit.SetPosition(
			GameCoordinatesAdapter.PixelsToGameUnits(_body.GlobalPosition.X),
			GameCoordinatesAdapter.PixelsToGameUnits(_body.GlobalPosition.Y));

	}

	private void OnVelocityComputed(Vector2 safeVelocity)
	{
		_safeVelocity = safeVelocity;
	}

	public void TakeDamage(int amount, UnitLogic attacker = null, bool isAoe = false)
	{
		if (!IsAlive) return;

		float atkX = 0, atkY = 0;
		if (attacker != null && attacker._body != null)
		{
			atkX = GameCoordinatesAdapter.PixelsToGameUnits(attacker._body.GlobalPosition.X);
			atkY = GameCoordinatesAdapter.PixelsToGameUnits(attacker._body.GlobalPosition.Y);
		}
		_unit.TakeDamage(amount, attacker?.Handle, atkX, atkY, isAoe);
	}

	private Vector2 SelectTarget()
	{
			IBuildingRef tavern = _unit.AssignedTavern;
			if (tavern != null)
			{
				(float centerX, float centerY) = TavernPartyService.GetTavernStandTarget(_unit, tavern);
			return new Vector2(
				GameCoordinatesAdapter.GameUnitsToPixels(centerX),
				GameCoordinatesAdapter.GameUnitsToPixels(centerY));
			}

		GameManager gm = AdapterRegistry.Resolve<GameManager>();
		Castle targetCastle = IsPlayerUnit ? gm?.EnemyCastle : gm?.PlayerCastle;

		if (targetCastle != null && targetCastle.IsAlive)
			return targetCastle.Heart.GlobalPosition;

		if (targetCastle != null)
			return targetCastle.GlobalPosition + new Vector2(
				targetCastle.GridColumns * targetCastle.CellSize / 2f,
				targetCastle.GridRows * targetCastle.CellSize / 2f);

		float dir = IsPlayerUnit ? GameCoordinatesAdapter.PixelsPerCell : -GameCoordinatesAdapter.PixelsPerCell;
		return new Vector2(_body.GlobalPosition.X + dir, _body.GlobalPosition.Y);
	}

	private void OnDamaged()
	{
		_visual.SetHealth(Health, MaxHealth);
		_visual?.StartHitFlash();
		Sfx.Play(GetHitSfxEvent(_eventRelay.LastAttacker));
		IUnitHandle attacker = _eventRelay.LastAttacker;
		if (attacker != null && attacker.IsAlive)
			_tactical.PropagateRetaliation(_unit, attacker);
	}

	private string GetAttackSfxEvent()
	{
		if (AttackType == AttackType.Melee)
		{
			switch (DamageType)
			{
				case DamageType.Slash:
					return SfxEvents.MeleeSlash;
				case DamageType.Pierce:
					return SfxEvents.MeleePierce;
				default:
					return SfxEvents.MeleeAttack;
			}
		}

		switch (DamageType)
		{
			case DamageType.Pierce:
				return SfxEvents.LaunchArrow;
			case DamageType.Magic:
			case DamageType.Shadow:
				return SfxEvents.LaunchMagic;
			default:
				return SfxEvents.ProjectileLaunch;
		}
	}

	private static string GetHitSfxEvent(IUnitHandle attacker)
	{
		if (attacker == null)
			return SfxEvents.Hit;

		switch (attacker.DamageType)
		{
			case DamageType.Slash:
				return SfxEvents.HitSlash;
			case DamageType.Pierce:
				return SfxEvents.HitPierce;
			case DamageType.Magic:
			case DamageType.Shadow:
				return SfxEvents.HitMagic;
			default:
				return SfxEvents.Hit;
		}
	}

	private void OnDied()
	{
		_isDead = true;
		Shop shop = AdapterRegistry.Resolve<Shop>();
		if (shop != null && shop.FactionId == FactionDef.NecromancerId)
			GameManager.Get<BlackMagicService>()?.OnUnitDied(IsPlayerUnit, _unit?.Race);
		_visual?.UpdateSleepVisual(false, false);
		_lifecycle?.PlayDeathAnimation(_body);
		Sfx.Play(SfxEvents.UnitDeath);
		TavernPartyService tavernParty = GameManager.Get<TavernPartyService>();
		tavernParty?.RemoveOnDeath(_unit);
		_corpseService?.ScheduleCorpseRemoval(_unit);
	}

	private void OnHealed()
	{
		if (_body == null || _eventRelay == null)
			return;

		Node root = GetTree().CurrentScene;
		if (root == null)
			return;

		Projectile projectile = new Projectile();
		root.AddChild(projectile);
		projectile.SetModulate(new Color(0.5f, 1f, 0.6f));
		projectile.PlayHitSound = false;

		Vector2 fromPos = _body.GlobalPosition;
		Vector2 toPos = new Vector2(
			GameCoordinatesAdapter.GameUnitsToPixels(_eventRelay.LastHealTargetX),
			GameCoordinatesAdapter.GameUnitsToPixels(_eventRelay.LastHealTargetY));
		Texture2D texture = LoadProjectileTexture("res://assets/art/projectiles/magic_ball.png");
		if (texture == null)
			return;

		projectile.Launch(fromPos, toPos, ProjectileRules.SpeedHeal, texture);
	}

	private static System.Collections.Generic.Dictionary<string, Texture2D> _projectileTextures;

	private void OnAttack()
	{
		if (_body == null || _eventRelay == null)
			return;

		Node root = GetTree().CurrentScene;
		if (root == null)
			return;

		Sfx.Play(GetAttackSfxEvent());
		Projectile projectile = new Projectile();
		root.AddChild(projectile);
		projectile.PlayHitSound = AttackType == AttackType.Ranged;

		Vector2 fromPos = _body.GlobalPosition;
		float targetPixelX = GameCoordinatesAdapter.GameUnitsToPixels(_eventRelay.LastAttackTargetX);
		float targetPixelY = GameCoordinatesAdapter.GameUnitsToPixels(_eventRelay.LastAttackTargetY);
		Vector2 toPos = new Vector2(targetPixelX, targetPixelY);

		string texturePath = GetProjectileTexturePath();
		Texture2D texture = LoadProjectileTexture(texturePath);
		if (texture == null)
			return;

		float speed = AttackType == AttackType.Melee ? ProjectileRules.SpeedMelee : ProjectileRules.SpeedRanged;
		projectile.Launch(fromPos, toPos, speed, texture);
	}

	private string GetProjectileTexturePath()
	{
		if (AttackType == AttackType.Ranged)
		{
			if (DamageType == DamageType.Magic || DamageType == DamageType.Shadow)
				return "res://assets/art/projectiles/magic_ball.png";
			return "res://assets/art/projectiles/arrow.png";
		}

		if (DamageType == DamageType.Pierce)
			return "res://assets/art/projectiles/spear.png";
		return "res://assets/art/projectiles/sword.png";
	}

	private static Texture2D LoadProjectileTexture(string path)
	{
		if (_projectileTextures == null)
			_projectileTextures = new System.Collections.Generic.Dictionary<string, Texture2D>();

		if (_projectileTextures.TryGetValue(path, out Texture2D cached))
			return cached;

		Texture2D texture = GD.Load<Texture2D>(path);
		if (texture != null)
			_projectileTextures[path] = texture;
		return texture;
	}

	private static float ComputeDisplaySize(UnitSize size)
	{
		switch (size)
		{
			case UnitSize.Small: return SmallDisplaySize;
			case UnitSize.Medium: return MediumDisplaySize;
			case UnitSize.Large: return LargeDisplaySize;
			case UnitSize.Huge: return HugeDisplaySize;
			default: return MediumDisplaySize;
		}
	}

}
