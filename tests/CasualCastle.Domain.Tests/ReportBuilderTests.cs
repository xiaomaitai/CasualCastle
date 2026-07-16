using System.Collections.Generic;
using CasualCastle.Domain.History;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class ReportBuilderTests
{
	[Fact]
	public void CaptureSnapshot_ExcludesCoreBuildings()
	{
		List<BuildingSnapshot> source = new()
		{
			new BuildingSnapshot { TypeId = "CastleHeart", AnchorGridX = 0, AnchorGridY = 0, Health = 100 },
			new BuildingSnapshot { TypeId = "Barracks", AnchorGridX = 2, AnchorGridY = 3, Health = 80 },
		};

		CastleSnapshot snapshot = ReportBuilder.CaptureSnapshot(source, 1, typeId => typeId == "CastleHeart");

		Assert.Single(snapshot.Buildings);
		Assert.Equal("Barracks", snapshot.Buildings[0].TypeId);
		Assert.Equal(1, snapshot.NightIndex);
	}

	[Fact]
	public void CaptureSnapshot_PreservesBuildingProperties()
	{
		List<BuildingSnapshot> source = new()
		{
			new BuildingSnapshot
			{
				TypeId = "Barracks", AnchorGridX = 3, AnchorGridY = 5, Health = 60,
				IsManuallyPaused = true, IsCombineProhibited = false
			},
		};

		CastleSnapshot snapshot = ReportBuilder.CaptureSnapshot(source, 2, _ => false);

		Assert.Single(snapshot.Buildings);
		BuildingSnapshot building = snapshot.Buildings[0];
		Assert.Equal("Barracks", building.TypeId);
		Assert.Equal(3, building.AnchorGridX);
		Assert.Equal(5, building.AnchorGridY);
		Assert.Equal(60, building.Health);
		Assert.True(building.IsManuallyPaused);
		Assert.False(building.IsCombineProhibited);
	}

	[Fact]
	public void CreateReport_GeneratesIdAndTimestamp()
	{
		List<CastleSnapshot> nights = new()
		{
			new CastleSnapshot { NightIndex = 1 },
		};

		BattleReport report = ReportBuilder.CreateReport(nights, "Test Report");

		Assert.NotNull(report.ReportId);
		Assert.NotEmpty(report.ReportId);
		Assert.Equal("Test Report", report.DisplayName);
		Assert.Single(report.Nights);
		Assert.Equal(1, report.Nights[0].NightIndex);
		Assert.True(report.SavedAtUnix > 0);
	}

	[Fact]
	public void CloneBuildingSnapshot_ReturnsDeepCopy()
	{
		BuildingSnapshot source = new()
		{
			TypeId = "Barracks", AnchorGridX = 1, AnchorGridY = 2, Health = 100,
			IsManuallyPaused = true, IsCombineProhibited = true,
		};

		BuildingSnapshot clone = ReportBuilder.CloneBuildingSnapshot(source);

		Assert.Equal(source.TypeId, clone.TypeId);
		Assert.Equal(source.AnchorGridX, clone.AnchorGridX);
		Assert.Equal(source.Health, clone.Health);
		Assert.NotSame(source, clone);
	}

	[Fact]
	public void CloneSnapshot_ReturnsDeepCopyWithIndependentBuildings()
	{
		CastleSnapshot source = new() { NightIndex = 2 };
		source.Buildings.Add(new BuildingSnapshot { TypeId = "Barracks", AnchorGridX = 3, AnchorGridY = 4, Health = 50 });

		CastleSnapshot clone = ReportBuilder.CloneSnapshot(source);

		Assert.Equal(source.NightIndex, clone.NightIndex);
		Assert.Single(clone.Buildings);
		Assert.NotSame(source.Buildings[0], clone.Buildings[0]);
	}
}
