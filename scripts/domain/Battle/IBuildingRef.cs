namespace CasualCastle.Domain.Battle;

public interface IBuildingRef
{
    bool IsDestroyed { get; }
    bool IsEnemyOf(ISoldierService soldier);
    float MinX { get; }
    float MinY { get; }
    float MaxX { get; }
    float MaxY { get; }
    IBuildingTarget BuildingTarget { get; }
    object CastleRef { get; }
}
