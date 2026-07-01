namespace CasualCastle.Domain.Battle;

public class SoldierService : ISoldierService
{
    internal Soldier Aggregate { get; }
    public ISoldierEventPort EventPort { get; set; }

    public INavigationPort NavPort
    {
        get => Aggregate.NavPort;
        set => Aggregate.NavPort = value;
    }

    public SoldierService()
    {
        Aggregate = new Soldier();
    }

    public bool IsAlive => Aggregate.IsAlive;
    public bool IsPlayerUnit => Aggregate.IsPlayerUnit;
    public int Health => Aggregate.Health;
    public int MaxHealth => Aggregate.MaxHealth;
    public int Damage => Aggregate.Damage;
    public float Speed => Aggregate.Speed;
    public float AttackRange => Aggregate.AttackRange;
    public float AttackCooldown => Aggregate.AttackCooldown;
    public float VisionRange => Aggregate.VisionRange;
    public float CollisionRadius => Aggregate.CollisionRadius;
    public bool HasNightCombat { get => Aggregate.HasNightCombat; set => Aggregate.HasNightCombat = value; }
    public float GameX => Aggregate.GameX;
    public float GameY => Aggregate.GameY;
    public SoldierState State => Aggregate.State;
    public ArmorType ArmorType => Aggregate.ArmorType;

    public void Initialize(UnitStats stats, bool isPlayerUnit)
    {
        Aggregate.Initialize(stats, isPlayerUnit);
    }

    public void SetEnemyTarget(ISoldierService target)
    {
        Aggregate.TargetEnemy = target;
    }

    public void UpdateTargeting(ISoldierService nearestEnemy, float enemyEdgeDist)
    {
        Aggregate.UpdateTargeting(nearestEnemy, enemyEdgeDist);
    }

    public void UpdateBehavior(float dt, float enemyEdgeDist, float marchGameX, float marchGameY)
    {
        Aggregate.UpdateBehavior(dt, enemyEdgeDist, marchGameX, marchGameY, this);
    }

    public void TakeDamage(int amount, ISoldierService attacker, float attackerGameX, float attackerGameY)
    {
        Aggregate.TakeDamage(amount, attacker, attackerGameX, attackerGameY);
        EventPort?.OnDamaged(amount, attacker);
        if (!Aggregate.IsAlive)
            EventPort?.OnDied();
    }

    public void MoveTo(float gameX, float gameY)
    {
        Aggregate.GameX = gameX;
        Aggregate.GameY = gameY;
    }

    public void ApplyPush(float dx, float dy)
    {
        Aggregate.GameX += dx;
        Aggregate.GameY += dy;
    }

    public void SetBuildingTarget(IBuildingTarget building)
    {
        Aggregate.TargetBuilding = building;
    }

    public void ClearBuildingTarget()
    {
        Aggregate.TargetBuilding = null;
    }
}
