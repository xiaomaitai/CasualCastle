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

    public void CaptureNightSnapshot(Castle castle, int nightIndex)
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

        _service.CaptureNightSnapshot(snapshots, nightIndex);
    }

    public void DiscardCurrentReport()
    {
        _service.DiscardCurrentReport();
    }

    public BattleReport SaveCurrentReport()
    {
        return _service.SaveCurrentReport("", DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm"));
    }

    public CastleSnapshot GetSelectedNightSnapshot(int nightIndex)
    {
        return _service.GetSelectedNightSnapshot(nightIndex);
    }

    public void ReloadSavedReports()
    {
        _service.ReloadSavedReports();
    }
}
