// 战报系统：快照采集与战报读写入口
using CasualCastle.Domain.Building;
using CasualCastle.Domain.History;
using CasualCastle.Domain.Shared;
using CasualCastle.Adapters.Godot;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BattleReportSystem : Node, IBattleReportCapture
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

	public void CapturePhaseSnapshot(IReadOnlyList<IBuildingState> buildings, int gold, IReadOnlyList<CardData> handCards, int nightIndex, BattlePhase phase)
	{
		if (nightIndex <= 0)
			return;

		IEnumerable<BuildingSnapshot> snapshots = buildings
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

		_service.CapturePhaseSnapshot(snapshots, nightIndex, phase, gold, handCards);
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
		return AdapterRegistry.Resolve<GameManager>()?.SelectedFaction ?? FactionIds.Alliance;
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
