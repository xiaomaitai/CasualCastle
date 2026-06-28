using CasualCastle.Domain.Shared;
using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public interface IBuildingRegistry
{
    BuildingData Get(string typeId);
    IReadOnlyList<GridCellOffset> GetFootprint(string typeId);
    GridCellOffset GetMainCellOffset(string typeId);
    string GetDisplayName(string typeId);
    int GetMaxHealth(string typeId);
    float GetSpawnInterval(string typeId);
    GridCellOffset GetSpawnCellOffset(string typeId);
    bool GetHasNightCombat(string typeId);
    int GetFusionTier(string typeId);
    bool IsCoreBuilding(string typeId);
    bool IsFusibleMaterial(string typeId);
}
