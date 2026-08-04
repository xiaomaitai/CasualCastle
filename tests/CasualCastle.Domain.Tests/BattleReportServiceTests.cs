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
	public void SaveCurrentReportDaily_FirstCall_CreatesSingleSavedReport()
	{
		BattleReportService service = CreateService(out FakeReportRepository repository);
		CapturePhase(service, 1, BattlePhase.Day);

		BattleReport saved = service.SaveCurrentReportDaily("progress name");

		Assert.Single(repository.Reports);
		Assert.Same(saved, repository.Reports[0]);
		Assert.True(service.HasCurrentSnapshots);
		Assert.Single(saved.Nights);
		Assert.Equal(1, saved.Nights[0].NightIndex);
		Assert.Equal(BattlePhase.Day, saved.Nights[0].Phase);
	}

	[Fact]
	public void SaveCurrentReportDaily_SecondCall_UpdatesSameReport()
	{
		BattleReportService service = CreateService(out FakeReportRepository repository);
		CapturePhase(service, 1, BattlePhase.Day);
		CapturePhase(service, 1, BattlePhase.Night);
		BattleReport first = service.SaveCurrentReportDaily("progress name");
		CapturePhase(service, 2, BattlePhase.Day);

		BattleReport second = service.SaveCurrentReportDaily("progress name");

		Assert.Single(repository.Reports);
		Assert.Equal(first.ReportId, second.ReportId);
		Assert.Equal(3, second.Nights.Count);
		Assert.True(service.HasCurrentSnapshots);
	}

	[Fact]
	public void SaveCurrentReportDaily_NoSnapshots_ReturnsNullAndSavesNothing()
	{
		BattleReportService service = CreateService(out FakeReportRepository repository);

		BattleReport saved = service.SaveCurrentReportDaily("progress name");

		Assert.Null(saved);
		Assert.Empty(repository.Reports);
	}

	[Fact]
	public void SaveCurrentReportDaily_DoesNotChangeSelectedReport()
	{
		BattleReportService service = CreateService(out FakeReportRepository _);
		service.StartMatch("reference-report");
		CapturePhase(service, 1, BattlePhase.Day);

		service.SaveCurrentReportDaily("progress name");

		Assert.Equal("reference-report", service.SelectedReportId);
	}

	[Fact]
	public void SaveCurrentReport_AfterDailySave_FinalizesSameReport()
	{
		BattleReportService service = CreateService(out FakeReportRepository repository);
		CapturePhase(service, 1, BattlePhase.Day);
		CapturePhase(service, 1, BattlePhase.Night);
		service.SaveCurrentReportDaily("progress name");
		CapturePhase(service, 1, BattlePhase.Combine);

		BattleReport saved = service.SaveCurrentReport("", "final name");

		Assert.Single(repository.Reports);
		Assert.Equal("final name", saved.DisplayName);
		Assert.Equal(3, saved.Nights.Count);
		Assert.False(service.HasCurrentSnapshots);
	}

	[Fact]
	public void SaveCurrentReport_WithoutDailySave_CreatesSingleReport()
	{
		BattleReportService service = CreateService(out FakeReportRepository repository);
		CapturePhase(service, 1, BattlePhase.Night);

		BattleReport saved = service.SaveCurrentReport("", "final name");

		Assert.Single(repository.Reports);
		Assert.Same(saved, repository.Reports[0]);
	}

	[Fact]
	public void SaveCurrentReport_DoesNotChangeSelectedReport()
	{
		BattleReportService service = CreateService(out FakeReportRepository _);
		service.StartMatch("reference-report");
		CapturePhase(service, 1, BattlePhase.Day);

		service.SaveCurrentReport("", "final name");

		Assert.Equal("reference-report", service.SelectedReportId);
	}

	[Fact]
	public void DiscardCurrentReport_RemovesDailySavedReportAndClearsCache()
	{
		BattleReportService service = CreateService(out FakeReportRepository repository);
		CapturePhase(service, 1, BattlePhase.Day);
		service.SaveCurrentReportDaily("progress name");

		service.DiscardCurrentReport();

		Assert.Empty(repository.Reports);
		Assert.False(service.HasCurrentSnapshots);
	}

	[Fact]
	public void CapturePhaseSnapshot_StoresAllThreePhasesForSameCycle()
	{
		BattleReportService service = CreateService(out FakeReportRepository _);
		service.StartMatch("");
		CapturePhase(service, 1, BattlePhase.Day);
		CapturePhase(service, 1, BattlePhase.Night);
		CapturePhase(service, 1, BattlePhase.Combine);

		BattleReport saved = service.SaveCurrentReportDaily("report name");

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

		BattleReport saved = service.SaveCurrentReportDaily("report name");

		Assert.Equal(2, saved.Nights.Count);
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
		BattleReport saved = service.SaveCurrentReportDaily("report name");

		service.StartMatch(saved.ReportId);

		Assert.NotNull(service.GetSelectedPhaseSnapshot(1, BattlePhase.Day));
		Assert.NotNull(service.GetSelectedPhaseSnapshot(1, BattlePhase.Night));
		Assert.NotNull(service.GetSelectedPhaseSnapshot(1, BattlePhase.Combine));
		Assert.NotNull(service.GetSelectedPhaseSnapshot(2, BattlePhase.Day));
		Assert.Null(service.GetSelectedPhaseSnapshot(2, BattlePhase.Night));
		Assert.Null(service.GetSelectedPhaseSnapshot(3, BattlePhase.Day));
	}

	private static BattleReportService CreateService(out FakeReportRepository repository)
	{
		repository = new FakeReportRepository();
		Mock<IBuildingRepository> buildingRepo = new();
		buildingRepo.Setup(r => r.Get(It.IsAny<string>())).Returns(new BuildingData
		{
			TypeId = "Barracks",
			IsCore = false,
		});
		return new BattleReportService(repository, buildingRepo.Object);
	}

	private static void CapturePhase(BattleReportService service, int nightIndex, BattlePhase phase)
	{
		service.CapturePhaseSnapshot(new[]
		{
			new BuildingSnapshot { TypeId = "Barracks", AnchorGridX = 1, AnchorGridY = 1, Health = 100 },
		}, nightIndex, phase);
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
