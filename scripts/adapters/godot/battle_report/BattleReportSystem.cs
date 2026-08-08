using CasualCastle.Domain.Building;
using CasualCastle.Domain.History;
using CasualCastle.Adapters.Godot;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BattleReportSystem : Node
{
    private BattleReportService _service;

    public bool HasCurrentSnapshots => _service.HasCurrentSnapshots;
    public string SelectedReportId => _service.SelectedReportId;
    public IReadOnlyList<BattleReport> SavedReports => _service.SavedReports;

    public override void _Ready()
    {
        AdapterRegistry.Register<BattleReportSystem>(this);
        _service = GameManager.Get<BattleReportService>();
    }

    public override void _ExitTree()
    {
        AdapterRegistry.Unregister<BattleReportSystem>(this);
    }

    public void StartMatch(string selectedReportId)
    {
        _service.StartMatch(selectedReportId);
    }

    public void CapturePhaseSnapshot(Castle castle, int nightIndex, BattlePhase phase)
    {
        if (castle == null || nightIndex <= 0)
            return;

        IEnumerable<BuildingSnapshot> snapshots = castle.GetBuildings()
            .Where(b => !GameManager.Get<IBuildingRepository>().IsCoreBuilding(b.TypeId) && !b.IsDestroyed)
            .Select(b => new BuildingSnapshot
            {
                TypeId = b.TypeId,
                AnchorGridX = b.AnchorGridX,
                AnchorGridY = b.AnchorGridY,
                Health = b.Health,
                IsManuallyPaused = b.IsManuallyPaused,
                IsCombineProhibited = b.IsCombineProhibited,
            });

        (int gold, IReadOnlyList<CardData> handCards) = CapturePlayerEconomy();
        _service.CapturePhaseSnapshot(snapshots, nightIndex, phase, gold, handCards);
    }

    private static (int gold, IReadOnlyList<CardData> handCards) CapturePlayerEconomy()
    {
        Shop shop = AdapterRegistry.Resolve<Shop>();
        Hand hand = AdapterRegistry.Resolve<Hand>();
        int gold = shop == null ? 0 : shop.Gold;
        IReadOnlyList<CardData> handCards = hand == null
            ? System.Array.Empty<CardData>()
            : hand.Cards;
        return (gold, handCards);
    }

    public void DiscardCurrentReport()
    {
        _service.DiscardCurrentReport();
    }

    public void ClearCurrentCache()
    {
        _service.ClearCurrentCache();
    }

    public BattleReport SaveCurrentReport()
    {
        return _service.SaveCurrentReport("", DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm"), GetCurrentFactionId());
    }

    public BattleReport SaveCurrentReportDaily()
    {
        return _service.SaveCurrentReportDaily(DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm"), GetCurrentFactionId());
    }

    private static string GetCurrentFactionId()
    {
        return AdapterRegistry.Resolve<GameManager>()?.SelectedFaction ?? "alliance";
    }

    public CastleSnapshot GetSelectedPhaseSnapshot(int nightIndex, BattlePhase phase)
    {
        return _service.GetSelectedPhaseSnapshot(nightIndex, phase);
    }

    public void ReloadSavedReports()
    {
        _service.ReloadSavedReports();
    }
}
