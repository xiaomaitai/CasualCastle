using System;
using System.Collections.Generic;
using System.Linq;

namespace CasualCastle.Domain.History;

public static class ReportBuilder
{
    public static CastleSnapshot CaptureSnapshot(
        IEnumerable<BuildingSnapshot> sourceBuildings, int nightIndex,
        System.Func<string, bool> isCoreBuilding)
    {
        CastleSnapshot snapshot = new()
        {
            NightIndex = nightIndex,
        };

        foreach (BuildingSnapshot building in sourceBuildings)
        {
            if (isCoreBuilding(building.TypeId))
                continue;

            snapshot.Buildings.Add(CloneBuildingSnapshot(building));
        }

        return snapshot;
    }

    public static BattleReport CreateReport(
        IReadOnlyList<CastleSnapshot> nights, string displayName)
    {
        DateTimeOffset now = DateTimeOffset.Now;
        return new BattleReport
        {
            ReportId = Guid.NewGuid().ToString("N"),
            DisplayName = displayName,
            SavedAtUnix = now.ToUnixTimeSeconds(),
            Nights = nights.Select(CloneSnapshot).ToList(),
        };
    }

    public static CastleSnapshot CloneSnapshot(CastleSnapshot source)
    {
        return new CastleSnapshot
        {
            NightIndex = source.NightIndex,
            Buildings = source.Buildings
                .Select(CloneBuildingSnapshot)
                .ToList(),
        };
    }

    public static BuildingSnapshot CloneBuildingSnapshot(BuildingSnapshot source)
    {
        return new BuildingSnapshot
        {
            TypeId = source.TypeId,
            AnchorGridX = source.AnchorGridX,
            AnchorGridY = source.AnchorGridY,
            Health = source.Health,
            IsManuallyPaused = source.IsManuallyPaused,
            IsFusionProhibited = source.IsFusionProhibited,
        };
    }
}
