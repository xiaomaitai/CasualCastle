using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public interface IFieldUnitRepository
{
    void Register(ISoldierService soldier);
    void Unregister(ISoldierService soldier);
    IReadOnlyList<ISoldierService> AllUnits { get; }
    (ISoldierService nearest, float edgeDist) FindNearestEnemy(ISoldierService soldier);
    void PropagateRetaliation(ISoldierService center, ISoldierService attacker);

    void RegisterBuilding(IBuildingRef building);
    void UnregisterBuilding(IBuildingRef building);
    (IBuildingTarget building, object castle) FindOverlappingBuilding(ISoldierService soldier);
    bool HasEnemyOnBuilding(IBuildingRef building);
}
