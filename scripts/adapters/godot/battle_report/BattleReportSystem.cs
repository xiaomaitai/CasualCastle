using CasualCastle.Domain.Building;
using CasualCastle.Domain.History;
using CasualCastle.Adapters.Godot;
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

    private IBattleReportRepository _repository;

    public IReadOnlyList<BattleReport> GetSavedReports() => _savedReports;
    public bool HasCurrentSnapshots => _currentReport.Nights.Count > 0;
    public string SelectedReportId => _selectedReportId;

    public override void _Ready()
    {
        Instance = this;
        AdapterRegistry.Register<BattleReportSystem>(this);
        _repository = GameManager.Get<IBattleReportRepository>();
        ReloadSavedReports();
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            AdapterRegistry.Unregister<BattleReportSystem>(this);
            Instance = null;
        }
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

        IEnumerable<BuildingSnapshot> sourceSnapshots = castle.GetBuildings()
            .Where(b => !BuildingDefinitions.IsCoreBuilding(b.TypeId) && !b.IsDestroyed)
            .Select(b => new BuildingSnapshot
            {
                TypeId = b.TypeId,
                AnchorGridX = b.AnchorGridX,
                AnchorGridY = b.AnchorGridY,
                Health = b.Health,
                IsManuallyPaused = b.IsManuallyPaused,
                IsFusionProhibited = b.IsFusionProhibited,
            });

        CastleSnapshot snapshot = ReportBuilder.CaptureSnapshot(
            sourceSnapshots, nightIndex, BuildingDefinitions.IsCoreBuilding);

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
        string name = string.IsNullOrWhiteSpace(displayName)
            ? BattleReportStorage.Instance?.BuildDefaultName(now) ?? now.ToString("yyyy-MM-dd HH:mm")
            : displayName;

        BattleReport toSave = ReportBuilder.CreateReport(_currentReport.Nights, name);

        _savedReports.Add(toSave);
        _savedReports.Sort((a, b) => b.SavedAtUnix.CompareTo(a.SavedAtUnix));
        _repository?.SaveAll(_savedReports);

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
        if (_repository != null)
            _savedReports.AddRange(_repository.LoadAll());
        _savedReports.Sort((a, b) => b.SavedAtUnix.CompareTo(a.SavedAtUnix));
    }
}
