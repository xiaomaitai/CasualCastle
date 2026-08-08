// 战斗单位聚合：状态、属性、位置与目标设置
using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public partial class Unit : IUnitHandle
{
	internal INavigationPort NavPort { get; }

	public IUnitEventPort EventPort { get; set; }

	public string TypeId { get; set; }

	public bool IsPlayerUnit { get; set; }

	public bool IsAlive { get; set; } = true;

	public float GameX { get; internal set; }

	public float GameY { get; internal set; }

	public int Health { get; set; }

	public int MaxHealth { get; set; }

	public int Damage { get; set; }

	public int UnitCost { get; set; }

	public float Speed { get; set; }

	public float AttackRange { get; set; }

	public float AttackCooldown { get; set; }

	public float VisionRange { get; set; }

	public float CollisionRadius { get; set; }

	public DamageType DamageType { get; set; }

	public AttackType AttackType { get; set; }

	public ArmorType ArmorType { get; set; }

	public int ArmorValue { get; set; }

	public int EffectiveArmorValue => ArmorValue + GameCtx.AuraArmorBonus;

	public string Race { get; set; }

	public bool IsFurious { get; private set; }

	public bool IsAmbushing { get; private set; }

	public bool IsStealthed { get; private set; }

	public bool CanDetectStealth { get; set; }

	public bool HasNightCombat { get; set; }

	public float HealAmount { get; set; }

	public float HealRange { get; set; }

	public float HealInterval { get; set; }

	public int ShieldHealth { get; private set; }

	public ITacticalQueries Tactical { get; set; }

	public BlackMagicService BlackMagic { get; set; }

	public SkillSet Skills { get; private set; }

	public GameContext GameCtx { get; set; } = new();

	public IUnitHandle TargetEnemy { get; set; }

	public IUnitHandle HealTarget { get; set; }

	public IBuildingTarget TargetBuilding { get; set; }

	public IBuildingRef AssignedTavern { get; set; }

	public UnitState State { get; set; }

	private float _attackTimer;

	private float _healTimer;

	private DamageMatrix _damageMatrix;

	private readonly System.Random _rng = new();

	private readonly Dictionary<string, float> _skillCooldowns = new();

	private float _kiteTimer;

	private float _slowRemaining;

	private float _slowFactor;

	private float _curseRemaining;

	private float _curseFactor;

	private float _poisonDamagePerSecond;

	private float _poisonRemaining;

	private float _poisonAccumulator;

	private float _timeSinceLastCombat;

	private float _auraRegenAccumulator;

	private float _wildHeartAccumulator;

	public string TargetDescription
	{
		get
		{
			if (TargetEnemy != null && TargetEnemy.IsAlive)
				return $"敌方单位 ({TargetEnemy.GameX:F0}, {TargetEnemy.GameY:F0})";
			if (TargetBuilding != null)
				return "建筑";
			return "行军目标";
		}
	}

	public Unit(INavigationPort navPort)
	{
		NavPort = navPort;
	}

	public void Initialize(UnitStats stats, bool isPlayerUnit)
	{
		TypeId = stats.TypeId;
		IsPlayerUnit = isPlayerUnit;
		MaxHealth = stats.Health;
		Health = stats.Health;
		Damage = stats.Damage;
		UnitCost = stats.UnitCost;
		Speed = stats.Speed;
		AttackRange = stats.AttackRange;
		AttackCooldown = stats.AttackCooldown;
		VisionRange = stats.VisionRange;
		CollisionRadius = stats.CollisionRadius;
		DamageType = stats.DamageType;
		AttackType = stats.AttackType;
		ArmorType = stats.ArmorType;
		ArmorValue = stats.ArmorValue;
		Race = stats.Race;
		CanDetectStealth = stats.CanDetectStealth;
		HasNightCombat = stats.HasNightCombat;
		HealAmount = stats.HealAmount;
		HealRange = stats.HealRange;
		HealInterval = stats.HealInterval;
		Skills = new SkillSet();
	}

	public void SetGameContext(GameContext ctx)
	{
		GameCtx = ctx;
	}

	public void SetDamageMatrix(DamageMatrix dm)
	{
		_damageMatrix = dm;
	}

	public void SetPosition(float gameX, float gameY)
	{
		GameX = gameX;
		GameY = gameY;
	}

	public void SetEnemyTarget(IUnitHandle target)
	{
		TargetEnemy = target;
	}

	public void SetStealth(bool stealthed)
	{
		IsStealthed = stealthed;
	}

	public void SetBuildingTarget(IBuildingTarget building)
	{
		TargetBuilding = building;
	}

	public void ClearBuildingTarget()
	{
		TargetBuilding = null;
	}

}
