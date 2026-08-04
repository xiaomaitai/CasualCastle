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
    private string _inProgressReportId = "";

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
        _inProgressReportId = "";
    }

    public void CapturePhaseSnapshot(
        IEnumerable<BuildingSnapshot> buildingSnapshots, int nightIndex, BattlePhase phase)
    {
        if (nightIndex <= 0)
            return;

        CastleSnapshot snapshot = ReportBuilder.CaptureSnapshot(
            buildingSnapshots, nightIndex, phase, _buildingRepo.IsCoreBuilding);

        _currentReport.Nights.RemoveAll(s => s.NightIndex == nightIndex && s.Phase == phase);
        _currentReport.Nights.Add(snapshot);
        _currentReport.Nights.Sort((a, b) =>
        {
            int byNight = a.NightIndex.CompareTo(b.NightIndex);
            return byNight != 0 ? byNight : a.Phase.CompareTo(b.Phase);
        });
    }

    public void DiscardCurrentReport()
    {
        if (!string.IsNullOrEmpty(_inProgressReportId))
        {
            _savedReports.RemoveAll(r => r.ReportId == _inProgressReportId);
            _inProgressReportId = "";
            _repository.SaveAll(_savedReports);
        }

        _currentReport = new BattleReport();
    }

    public void ClearCurrentCache()
    {
        _currentReport = new BattleReport();
    }

    public BattleReport SaveCurrentReportDaily(string displayName)
    {
        if (_currentReport.Nights.Count == 0)
            return null;

        BattleReport report = _savedReports.FirstOrDefault(r => r.ReportId == _inProgressReportId);
        if (report == null)
        {
            report = ReportBuilder.CreateReport(_currentReport.Nights, displayName);
            _inProgressReportId = report.ReportId;
            _savedReports.Add(report);
        }
        else
        {
            report.SavedAtUnix = DateTimeOffset.Now.ToUnixTimeSeconds();
            report.Nights = _currentReport.Nights.Select(ReportBuilder.CloneSnapshot).ToList();
        }

        PersistSavedReports();
        return report;
    }

    public BattleReport SaveCurrentReport(string displayName, string defaultName)
    {
        if (_currentReport.Nights.Count == 0)
            return null;

        string name = string.IsNullOrWhiteSpace(displayName) ? defaultName : displayName;
        BattleReport toSave = ReportBuilder.CreateReport(_currentReport.Nights, name);

        BattleReport existing = _savedReports.FirstOrDefault(r => r.ReportId == _inProgressReportId);
        if (existing == null)
        {
            _savedReports.Add(toSave);
        }
        else
        {
            existing.DisplayName = toSave.DisplayName;
            existing.SavedAtUnix = toSave.SavedAtUnix;
            existing.Nights = toSave.Nights;
        }

        _inProgressReportId = "";
        _currentReport = new BattleReport();
        PersistSavedReports();
        return existing ?? toSave;
    }

    private void PersistSavedReports()
    {
        _savedReports.Sort((a, b) => b.SavedAtUnix.CompareTo(a.SavedAtUnix));
        _repository.SaveAll(_savedReports);
    }

    public BattleReport LoadReport(string reportId)
    {
        if (string.IsNullOrWhiteSpace(reportId))
            return null;
        return _savedReports.FirstOrDefault(r => r.ReportId == reportId);
    }

    public CastleSnapshot GetSelectedPhaseSnapshot(int nightIndex, BattlePhase phase)
    {
        if (nightIndex <= 0 || string.IsNullOrWhiteSpace(_selectedReportId))
            return null;
        BattleReport report = LoadReport(_selectedReportId);
        return report?.Nights.FirstOrDefault(s => s.NightIndex == nightIndex && s.Phase == phase);
    }

    public void ReloadSavedReports()
    {
        _savedReports.Clear();
        _savedReports.AddRange(_repository.LoadAll());
        _savedReports.Sort((a, b) => b.SavedAtUnix.CompareTo(a.SavedAtUnix));
    }
}
