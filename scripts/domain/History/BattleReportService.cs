using CasualCastle.Domain.Building;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CasualCastle.Domain.History;

public class BattleReportService
{
    private readonly IBattleReportRepository _repository;
    private readonly IBuildingRepository _buildingRepo;
    private readonly List<BattleReport> _savedReports = new();
    private BattleReport _currentReport = new();
    private string _selectedReportId = "";

    public IReadOnlyList<BattleReport> SavedReports => _savedReports;
    public bool HasCurrentSnapshots => _currentReport.Nights.Count > 0;
    public string SelectedReportId => _selectedReportId;

    public BattleReportService(IBattleReportRepository repository, IBuildingRepository buildingRepo)
    {
        _repository = repository;
        _buildingRepo = buildingRepo;
        ReloadSavedReports();
    }

    public void StartMatch(string selectedReportId)
    {
        _selectedReportId = selectedReportId ?? "";
        _currentReport = new BattleReport();
    }

    public void CaptureNightSnapshot(IEnumerable<BuildingSnapshot> buildingSnapshots, int nightIndex)
    {
        if (nightIndex <= 0)
            return;

        CastleSnapshot snapshot = ReportBuilder.CaptureSnapshot(
            buildingSnapshots, nightIndex, _buildingRepo.IsCoreBuilding);

        _currentReport.Nights.RemoveAll(s => s.NightIndex == nightIndex);
        _currentReport.Nights.Add(snapshot);
        _currentReport.Nights.Sort((a, b) => a.NightIndex.CompareTo(b.NightIndex));
    }

    public void DiscardCurrentReport()
    {
        _currentReport = new BattleReport();
    }

    public BattleReport SaveCurrentReport(string displayName, string defaultName)
    {
        if (_currentReport.Nights.Count == 0)
            return null;

        string name = string.IsNullOrWhiteSpace(displayName) ? defaultName : displayName;
        BattleReport toSave = ReportBuilder.CreateReport(_currentReport.Nights, name);

        _savedReports.Add(toSave);
        _savedReports.Sort((a, b) => b.SavedAtUnix.CompareTo(a.SavedAtUnix));
        _repository.SaveAll(_savedReports);

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
        _savedReports.AddRange(_repository.LoadAll());
        _savedReports.Sort((a, b) => b.SavedAtUnix.CompareTo(a.SavedAtUnix));
    }
}
