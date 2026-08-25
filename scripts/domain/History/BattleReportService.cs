// 战报服务：快照记录与战报保存
using CasualCastle.Domain.Shared;
using System.Collections.Generic;
using System.Linq;

namespace CasualCastle.Domain.History;

public class BattleReportService
{
	private readonly IBattleReportRepository _repository;
	private readonly IHistoryCoreLookup _coreLookup;
	private readonly List<BattleReport> _savedReports = new();
	private BattleReport _currentReport = new();
	private string _selectedReportId = "";

	public IReadOnlyList<BattleReport> SavedReports => _savedReports;
	public bool HasCurrentSnapshots => _currentReport.Nights.Count > 0;
	public string SelectedReportId => _selectedReportId;

	public BattleReportService(IBattleReportRepository repository, IHistoryCoreLookup coreLookup)
	{
		_repository = repository;
		_coreLookup = coreLookup;
		ReloadSavedReports();
	}

	public void StartMatch(string selectedReportId)
	{
		_selectedReportId = selectedReportId ?? "";
		_currentReport = new BattleReport();
	}

	public void CapturePhaseSnapshot(
		IEnumerable<BuildingSnapshot> buildingSnapshots, int nightIndex, BattlePhase phase,
		int gold, IEnumerable<CardSnapshot> handCards)
	{
		if (nightIndex <= 0)
			return;

		CastleSnapshot snapshot = ReportBuilder.CaptureSnapshot(
			buildingSnapshots, nightIndex, phase, _coreLookup.IsCoreBuilding, gold, handCards);

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
		_currentReport = new BattleReport();
	}

	public void ClearCurrentCache()
	{
		_currentReport = new BattleReport();
	}

	public IReadOnlyList<CastleSnapshot> ExportCurrentSnapshots()
	{
		return _currentReport.Nights.Select(ReportBuilder.CloneSnapshot).ToList();
	}

	public void ImportCurrentSnapshots(IReadOnlyList<CastleSnapshot> snapshots)
	{
		_currentReport.Nights.Clear();
		_currentReport.Nights.AddRange(snapshots.Select(ReportBuilder.CloneSnapshot));
	}

	public BattleReport SaveCurrentReport(string displayName, string defaultName, string factionId)
	{
		if (_currentReport.Nights.Count == 0)
			return null;

		string name = string.IsNullOrWhiteSpace(displayName) ? defaultName : displayName;
		BattleReport toSave = ReportBuilder.CreateReport(_currentReport.Nights, name, factionId);
		_savedReports.Add(toSave);
		_currentReport = new BattleReport();
		PersistSavedReports();
		return toSave;
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

	public bool IsNecromancerReport(string reportId)
	{
		return LoadReport(reportId)?.FactionId == FactionIds.Necromancer;
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
