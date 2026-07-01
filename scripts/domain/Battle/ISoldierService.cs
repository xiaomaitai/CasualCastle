namespace CasualCastle.Domain.Battle;

public interface ISoldierService
{
    bool IsAlive { get; }
    bool IsPlayerUnit { get; }
    int Health { get; }
    int Damage { get; }
    float Speed { get; }
    float AttackRange { get; }
    float AttackCooldown { get; }
    float VisionRange { get; }
    float CollisionRadius { get; }
    bool HasNightCombat { get; set; }
    float GameX { get; }
    float GameY { get; }
    SoldierState State { get; }
    ArmorType ArmorType { get; }

    void Initialize(UnitStats stats, bool isPlayerUnit);
    void SetEnemyTarget(ISoldierService target);
    void UpdateTargeting(ISoldierService nearestEnemy, float enemyEdgeDist);
    void UpdateBehavior(float dt, float enemyEdgeDist, float marchGameX, float marchGameY);
    void TakeDamage(int amount, ISoldierService attacker, float attackerGameX, float attackerGameY);
    void MoveTo(float gameX, float gameY);
    void ApplyPush(float dx, float dy);
    void SetBuildingTarget(IBuildingTarget building);
    void ClearBuildingTarget();
}
