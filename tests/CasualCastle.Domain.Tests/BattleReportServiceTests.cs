using System.Collections.Generic;
using System.Linq;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.History;
using Moq;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class BattleReportServiceTests
{
	[Fact]
	public void CapturingSnapshots_NeverPersistsUntilExplicitSave_RealBug_AutosavedWithoutAsking()
	{
		BattleReportService service = CreateService(out FakeReportRepository repository);
		service.StartMatch("");
		CapturePhase(service, 1, BattlePhase.Day);
		CapturePhase(service, 1, BattlePhase.Night);
		CapturePhase(service, 2, BattlePhase.Day);

		Assert.Empty(repository.Reports);
		Assert.True(service.HasCurrentSnapshots);
	}

	[Fact]
	public void SaveCurrentReport_NeverWritesWithoutExplicitCall_RealBug_AutosavedWithoutAsking()
	{
		BattleReportService service = CreateService(out FakeReportRepository repository);
		CapturePhase(service, 1, BattlePhase.Day);

		service.DiscardCurrentReport();

		Assert.Empty(repository.Reports);
		Assert.False(service.HasCurrentSnapshots);
	}

	[Fact]
	public void SaveCurrentReport_CreatesSingleReport()
	{
		BattleReportService service = CreateService(out FakeReportRepository repository);
		CapturePhase(service, 1, BattlePhase.Night);

		BattleReport saved = service.SaveCurrentReport("", "final name", "alliance");

		Assert.Single(repository.Reports);
		Assert.Same(saved, repository.Reports[0]);
		Assert.False(service.HasCurrentSnapshots);
	}

	[Fact]
	public void SaveCurrentReport_DoesNotChangeSelectedReport()
	{
		BattleReportService service = CreateService(out FakeReportRepository _);
		service.StartMatch("reference-report");
		CapturePhase(service, 1, BattlePhase.Day);

		service.SaveCurrentReport("", "final name", "alliance");

		Assert.Equal("reference-report", service.SelectedReportId);
	}

	[Fact]
	public void DiscardCurrentReport_ClearsCacheWithoutPersisting()
	{
		BattleReportService service = CreateService(out FakeReportRepository repository);
		CapturePhase(service, 1, BattlePhase.Day);

		service.DiscardCurrentReport();

		Assert.Empty(repository.Reports);
		Assert.False(service.HasCurrentSnapshots);
	}

	[Fact]
	public void ExportCurrentSnapshots_ReturnsClonedNights()
	{
		BattleReportService service = CreateService(out FakeReportRepository _);
		CapturePhase(service, 1, BattlePhase.Day);
		CapturePhase(service, 2, BattlePhase.Night);

		IReadOnlyList<CastleSnapshot> exported = service.ExportCurrentSnapshots();

		Assert.Equal(2, exported.Count);
		Assert.Equal(1, exported[0].NightIndex);
		Assert.Equal(BattlePhase.Day, exported[0].Phase);
		Assert.Equal(2, exported[1].NightIndex);
		Assert.Equal(BattlePhase.Night, exported[1].Phase);
		Assert.True(service.HasCurrentSnapshots);
	}

	[Fact]
	public void ImportCurrentSnapshots_RestoresCache()
	{
		BattleReportService service = CreateService(out FakeReportRepository _);
		List<CastleSnapshot> snapshots = new()
		{
			new CastleSnapshot { NightIndex = 3, Phase = BattlePhase.Combine },
			new CastleSnapshot { NightIndex = 4, Phase = BattlePhase.Day },
		};

		service.ImportCurrentSnapshots(snapshots);

		IReadOnlyList<CastleSnapshot> restored = service.ExportCurrentSnapshots();
		Assert.Equal(2, restored.Count);
		Assert.Equal(3, restored[0].NightIndex);
		Assert.Equal(BattlePhase.Combine, restored[0].Phase);
		Assert.Equal(4, restored[1].NightIndex);
		Assert.Equal(BattlePhase.Day, restored[1].Phase);
		Assert.True(service.HasCurrentSnapshots);
	}

	[Fact]
	public void CapturePhaseSnapshot_StoresAllThreePhasesForSameCycle()
	{
		BattleReportService service = CreateService(out FakeReportRepository _);
		service.StartMatch("");
		CapturePhase(service, 1, BattlePhase.Day);
		CapturePhase(service, 1, BattlePhase.Night);
		CapturePhase(service, 1, BattlePhase.Combine);

		BattleReport saved = service.SaveCurrentReport("", "report name", "alliance");

		Assert.Equal(3, saved.Nights.Count);
		Assert.All(saved.Nights, s => Assert.Equal(1, s.NightIndex));
		Assert.Equal(new[] { BattlePhase.Day, BattlePhase.Night, BattlePhase.Combine },
			saved.Nights.Select(s => s.Phase));
	}

	[Fact]
	public void CapturePhaseSnapshot_SameCycleAndPhase_ReplacesExistingSnapshot()
	{
		BattleReportService service = CreateService(out FakeReportRepository _);
		service.StartMatch("");
		CapturePhase(service, 1, BattlePhase.Night);
		CapturePhase(service, 1, BattlePhase.Night);
		CapturePhase(service, 1, BattlePhase.Day);

		BattleReport saved = service.SaveCurrentReport("", "report name", "alliance");

		Assert.Equal(2, saved.Nights.Count);
	}

	[Fact]
	public void CapturePhaseSnapshot_StoresGoldAndHandCards()
	{
		BattleReportService service = CreateService(out FakeReportRepository _);
		service.StartMatch("");
		List<CardSnapshot> cards = new()
		{
			new CardSnapshot { BuildingType = "Barracks", Cost = 3 },
		};

		service.CapturePhaseSnapshot(new[]
		{
			new BuildingSnapshot { TypeId = "Barracks", AnchorGridX = 1, AnchorGridY = 1, Health = 100 },
		}, 1, BattlePhase.Day, 25, cards);

		BattleReport saved = service.SaveCurrentReport("", "report name", "alliance");

		Assert.Equal(25, saved.Nights[0].Gold);
		CardSnapshot card = Assert.Single(saved.Nights[0].HandCards);
		Assert.Equal("Barracks", card.BuildingType);
		Assert.Equal(3, card.Cost);
	}

	[Fact]
	public void GetSelectedPhaseSnapshot_ReturnsOnlyMatchingPhase()
	{
		BattleReportService service = CreateService(out FakeReportRepository _);
		service.StartMatch("");
		CapturePhase(service, 1, BattlePhase.Day);
		CapturePhase(service, 1, BattlePhase.Night);
		CapturePhase(service, 1, BattlePhase.Combine);
		CapturePhase(service, 2, BattlePhase.Day);
		BattleReport saved = service.SaveCurrentReport("", "report name", "alliance");

		service.StartMatch(saved.ReportId);

		Assert.NotNull(service.GetSelectedPhaseSnapshot(1, BattlePhase.Day));
		Assert.NotNull(service.GetSelectedPhaseSnapshot(1, BattlePhase.Night));
		Assert.NotNull(service.GetSelectedPhaseSnapshot(1, BattlePhase.Combine));
		Assert.NotNull(service.GetSelectedPhaseSnapshot(2, BattlePhase.Day));
		Assert.Null(service.GetSelectedPhaseSnapshot(2, BattlePhase.Night));
		Assert.Null(service.GetSelectedPhaseSnapshot(3, BattlePhase.Day));
	}

	[Fact]
	public void IsNecromancerReport_ReportWithNecromancerFaction_ReturnsTrue()
	{
		BattleReportService service = CreateServiceWithReport(out FakeReportRepository _, CreateReportWithFaction(FactionDef.NecromancerId));

		Assert.True(service.IsNecromancerReport("report-1"));
	}

	[Fact]
	public void IsNecromancerReport_ReportWithAllianceFaction_ReturnsFalse()
	{
		BattleReportService service = CreateServiceWithReport(out FakeReportRepository _, CreateReportWithFaction("alliance"));

		Assert.False(service.IsNecromancerReport("report-1"));
	}

	[Fact]
	public void IsNecromancerReport_UnknownReportId_ReturnsFalse()
	{
		BattleReportService service = CreateService(out FakeReportRepository _);

		Assert.False(service.IsNecromancerReport("missing"));
	}

	private static BattleReport CreateReportWithFaction(string factionId)
	{
		return new BattleReport
		{
			ReportId = "report-1",
			DisplayName = "report",
			SavedAtUnix = 1,
			FactionId = factionId,
			Nights = new List<CastleSnapshot>
			{
				new CastleSnapshot { NightIndex = 1 },
			},
		};
	}

	private static BattleReportService CreateServiceWithReport(out FakeReportRepository repository, BattleReport report)
	{
		repository = new FakeReportRepository();
		repository.Reports.Add(report);
		Mock<IHistoryCoreLookup> coreLookup = new();
		coreLookup.Setup(r => r.IsCoreBuilding(It.IsAny<string>())).Returns(false);
		return new BattleReportService(repository, coreLookup.Object);
	}

	private static BattleReportService CreateService(out FakeReportRepository repository)
	{
		repository = new FakeReportRepository();
		Mock<IHistoryCoreLookup> coreLookup = new();
		coreLookup.Setup(r => r.IsCoreBuilding(It.IsAny<string>())).Returns(false);
		return new BattleReportService(repository, coreLookup.Object);
	}

	private static void CapturePhase(BattleReportService service, int nightIndex, BattlePhase phase)
	{
		service.CapturePhaseSnapshot(new[]
		{
			new BuildingSnapshot { TypeId = "Barracks", AnchorGridX = 1, AnchorGridY = 1, Health = 100 },
		}, nightIndex, phase, 0, new List<CardSnapshot>());
	}
}

internal sealed class FakeReportRepository : IBattleReportRepository
{
	private List<BattleReport> _reports = new();

	public List<BattleReport> Reports => _reports;

	public List<BattleReport> LoadAll()
	{
		return new List<BattleReport>(_reports);
	}

	public void SaveAll(IReadOnlyList<BattleReport> reports)
	{
		_reports = reports.ToList();
	}
}
