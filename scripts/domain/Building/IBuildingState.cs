namespace CasualCastle.Domain.Building;

public interface IBuildingState : IAdjacencyBuilding
{
    bool IsDestroyed { get; }
    bool IsManuallyPaused { get; }
    bool IsFusionProhibited { get; }
    bool HasEnemyOnTop { get; }
    bool IsPlayerOwned { get; }
}
