namespace CasualCastle.Domain.Building;

public interface IBuildingState : IAdjacencyBuilding
{
    bool IsDestroyed { get; }
    bool IsManuallyPaused { get; }
    bool IsCombineProhibited { get; }
    bool HasEnemyOnTop { get; }
    bool IsPlayerOwned { get; }
}
