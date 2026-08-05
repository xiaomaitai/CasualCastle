using CasualCastle.Domain.Building;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CasualCastle.Domain.History;

public static class ReportBuilder
{
    public static CastleSnapshot CaptureSnapshot(
        IEnumerable<BuildingSnapshot> sourceBuildings, int nightIndex, BattlePhase phase,
        System.Func<string, bool> isCoreBuilding, int gold, IEnumerable<CardData> handCards)
    {
        CastleSnapshot snapshot = new()
        {
            NightIndex = nightIndex,
            Phase = phase,
            Gold = gold,
            HandCards = handCards.Select(CreateCardSnapshot).ToList(),
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
            Phase = source.Phase,
            Buildings = source.Buildings
                .Select(CloneBuildingSnapshot)
                .ToList(),
            Gold = source.Gold,
            HandCards = source.HandCards
                .Select(CloneCardSnapshot)
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
            IsCombineProhibited = source.IsCombineProhibited,
        };
    }

    public static CardSnapshot CreateCardSnapshot(CardData card)
    {
        return new CardSnapshot
        {
            BuildingType = card.BuildingType,
            Cost = card.Cost,
        };
    }

    public static CardSnapshot CloneCardSnapshot(CardSnapshot source)
    {
        return new CardSnapshot
        {
            BuildingType = source.BuildingType,
            Cost = source.Cost,
        };
    }
}