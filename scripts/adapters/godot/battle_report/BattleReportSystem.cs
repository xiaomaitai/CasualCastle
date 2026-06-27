using CasualCastle.Domain.Ports;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BattleReportSystem : Node
{
    public static BattleReportSystem Instance { get; private set; }

    private readonly List<BattleReport> _savedReports = new();
    private BattleReport _currentReport = new();
    private string _selectedReportId = "";

    public IReadOnlyList<BattleReport> GetSavedReports() => _savedReports;
    public bool HasCurrentSnapshots => _currentReport.Nights.Count > 0;
    public string SelectedReportId => _selectedReportId;

    public override void _Ready()
    {
        Instance = this;
        ReloadSavedReports();
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }

    public void StartMatch(string selectedReportId)
    {
        _selectedReportId = selectedReportId ?? "";
        _currentReport = new BattleReport();
    }

    public void CaptureNightSnapshot(Castle castle, int nightIndex)
    {
        if (castle == null || nightIndex <= 0)
            return;

        CastleSnapshot snapshot = new()
        {
            NightIndex = nightIndex,
        };

        foreach (Building building in castle.GetBuildings())
        {
            if (BuildingSystem.IsCoreBuilding(building.TypeId) || building.IsDestroyed)
                continue;

            snapshot.Buildings.Add(new BuildingSnapshot
            {
                TypeId = building.TypeId,
                AnchorGridX = building.AnchorGridX,
                AnchorGridY = building.AnchorGridY,
                Health = building.Health,
                IsManuallyPaused = building.IsManuallyPaused,
                IsFusionProhibited = building.IsFusionProhibited,
            });
        }

        _currentReport.Nights.RemoveAll(s => s.NightIndex == nightIndex);
        _currentReport.Nights.Add(snapshot);
        _currentReport.Nights.Sort((a, b) => a.NightIndex.CompareTo(b.NightIndex));
    }

    public void DiscardCurrentReport()
    {
        _currentReport = new BattleReport();
    }

    public BattleReport SaveCurrentReport(string displayName = null)
    {
        if (_currentReport.Nights.Count == 0)
            return null;

        DateTimeOffset now = DateTimeOffset.Now;
        BattleReport toSave = new()
        {
            ReportId = Guid.NewGuid().ToString("N"),
            DisplayName = string.IsNullOrWhiteSpace(displayName)
                ? BattleReportStorage.Instance.BuildDefaultName(now)
                : displayName,
            SavedAtUnix = now.ToUnixTimeSeconds(),
            Nights = _currentReport.Nights
                .Select(CloneSnapshot)
                .ToList(),
        };

        _savedReports.Add(toSave);
        _savedReports.Sort((a, b) => b.SavedAtUnix.CompareTo(a.SavedAtUnix));
        BattleReportStorage.Instance.SaveAll(_savedReports);

        _selectedReportId = toSave.ReportId;
        _currentReport = new BattleReport();
        return toSave;
    }

    public BattleReport LoadReport(string reportId)
    {
        if (string.IsNullOrWhiteSpace(reportId))
            return null;

        return _savedReports.FirstOrDefault(r => r.ReportId == reportId);
    }

    public CastleSnapshot GetSelectedNightSnapshot(int nightIndex)
    {
        if (nightIndex <= 0 || string.IsNullOrWhiteSpace(_selectedReportId))
            return null;

        BattleReport report = LoadReport(_selectedReportId);
        return report?.Nights.FirstOrDefault(s => s.NightIndex == nightIndex);
    }

    public void ReloadSavedReports()
    {
        _savedReports.Clear();
        _savedReports.AddRange(BattleReportStorage.Instance.LoadAll());
        _savedReports.Sort((a, b) => b.SavedAtUnix.CompareTo(a.SavedAtUnix));
    }

    private static CastleSnapshot CloneSnapshot(CastleSnapshot source)
    {
        return new CastleSnapshot
        {
            NightIndex = source.NightIndex,
            Buildings = source.Buildings
                .Select(b => new BuildingSnapshot
                {
                    TypeId = b.TypeId,
                    AnchorGridX = b.AnchorGridX,
                    AnchorGridY = b.AnchorGridY,
                    Health = b.Health,
                    IsManuallyPaused = b.IsManuallyPaused,
                    IsFusionProhibited = b.IsFusionProhibited,
                })
                .ToList(),
        };
    }
}
