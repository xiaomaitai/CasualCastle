using System;

namespace CasualCastle.Domain.Battle;

public class Soldier
{
	private readonly IPositionAccessor _position;
	private readonly IPathAccessor _path;

	internal IPositionAccessor PositionAccessor => _position;
	internal IPathAccessor PathAccessor => _path;

	public string TypeId { get; set; }
	public bool IsPlayerUnit { get; set; }
	public bool IsAlive { get; set; } = true;

	public int Health { get; set; }
	public int MaxHealth { get; set; }
	public int Damage { get; set; }
	public float Speed { get; set; }
	public float AttackRange { get; set; }
	public float AttackCooldown { get; set; }
	public float VisionRange { get; set; }
	public float CollisionRadius { get; set; }
	public bool HasNightCombat { get; set; }
	public DamageType DamageType { get; set; }
	public ArmorType ArmorType { get; set; }

	public Soldier TargetEnemy { get; set; }
	public object TargetBuilding { get; set; }
	public object TargetCastle { get; set; }
	public SoldierState State { get; private set; }

	private float _attackTimer;

	public Soldier(IPositionAccessor position, IPathAccessor path)
	{
		_position = position;
		_path = path;
	}

	public void Initialize(UnitStats stats, bool isPlayerUnit)
	{
		TypeId = stats.TypeId;
		IsPlayerUnit = isPlayerUnit;
		MaxHealth = stats.Health;
		Health = stats.Health;
		Damage = stats.Damage;
		Speed = stats.Speed;
		AttackRange = stats.AttackRange;
		AttackCooldown = stats.AttackCooldown;
		VisionRange = stats.VisionRange;
		CollisionRadius = stats.CollisionRadius;
		HasNightCombat = stats.HasNightCombat;
		DamageType = stats.DamageType;
		ArmorType = stats.ArmorType;
	}

	public void TakeDamage(int amount, Soldier attacker)
	{
		if (!IsAlive)
			return;

		Health = CombatRules.ApplyDamage(Health, amount);

		if (Health <= 0)
		{
			IsAlive = false;
			return;
		}

		if (attacker != null && attacker.IsAlive && attacker.IsPlayerUnit != IsPlayerUnit)
		{
			float dx = _position.GameX - attacker._position.GameX;
			float dy = _position.GameY - attacker._position.GameY;
			float dist = MathF.Sqrt(dx * dx + dy * dy);
			if (dist > VisionRange)
				TargetEnemy = attacker;
		}
	}

	public void UpdateTargeting(Soldier nearestEnemy, float enemyEdgeDist)
	{
		if (!IsAlive)
			return;

		if (nearestEnemy != null && nearestEnemy.IsAlive)
			State = enemyEdgeDist <= VisionRange ? SoldierState.Fighting : SoldierState.Retaliating;
		else if (TargetBuilding != null)
			State = SoldierState.Sieging;
		else
			State = SoldierState.Marching;
	}

	public void UpdateBehavior(float dt, float enemyEdgeDist)
	{
		if (!IsAlive)
			return;

		if (_attackTimer > 0)
			_attackTimer -= dt;

		switch (State)
		{
			case SoldierState.Fighting:
			case SoldierState.Retaliating:
				if (TargetEnemy == null || !TargetEnemy.IsAlive)
					break;
				if (enemyEdgeDist <= AttackRange)
				{
					if (_attackTimer <= 0)
					{
						int finalDamage = CombatRules.CalculateDamage(Damage, DamageType, TargetEnemy.ArmorType);
						TargetEnemy.TakeDamage(finalDamage, this);
						_attackTimer = AttackCooldown;
					}
				}
				else
				{
					_path.SetTarget(TargetEnemy._position.GameX, TargetEnemy._position.GameY);
					MoveTowardPath(dt);
				}
				break;

			case SoldierState.Sieging:
				if (_attackTimer <= 0 && TargetBuilding != null)
					_attackTimer = AttackCooldown;
				break;

			case SoldierState.Marching:
				TargetEnemy = null;
				MoveTowardPath(dt);
				break;
		}
	}

	private void MoveTowardPath(float dt)
	{
		if (!_path.HasPath)
			return;

		float dx = _path.NextGameX - _position.GameX;
		float dy = _path.NextGameY - _position.GameY;
		float dist = MathF.Sqrt(dx * dx + dy * dy);
		if (dist < 0.001f)
			return;

		float moveAmount = Speed * dt;
		float ratio = moveAmount / dist;
		_position.GameX += dx * ratio;
		_position.GameY += dy * ratio;
	}
}
