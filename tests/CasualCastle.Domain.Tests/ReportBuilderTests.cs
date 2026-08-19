using System.Collections.Generic;
using CasualCastle.Domain.Building;
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

		CastleSnapshot snapshot = ReportBuilder.CaptureSnapshot(source, 1, BattlePhase.Day, typeId => typeId == "CastleHeart", 0, new List<CardSnapshot>());

		Assert.Single(snapshot.Buildings);
		Assert.Equal("Barracks", snapshot.Buildings[0].TypeId);
		Assert.Equal(1, snapshot.NightIndex);
		Assert.Equal(BattlePhase.Day, snapshot.Phase);
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

		CastleSnapshot snapshot = ReportBuilder.CaptureSnapshot(source, 2, BattlePhase.Night, _ => false, 0, new List<CardSnapshot>());

		Assert.Single(snapshot.Buildings);
		BuildingSnapshot building = snapshot.Buildings[0];
		Assert.Equal("Barracks", building.TypeId);
		Assert.Equal(3, building.AnchorGridX);
		Assert.Equal(5, building.AnchorGridY);
		Assert.Equal(60, building.Health);
		Assert.True(building.IsManuallyPaused);
		Assert.False(building.IsCombineProhibited);
		Assert.Equal(BattlePhase.Night, snapshot.Phase);
	}

	[Fact]
	public void CreateReport_GeneratesIdAndTimestamp()
	{
		List<CastleSnapshot> nights = new()
		{
			new CastleSnapshot { NightIndex = 1 },
		};

		BattleReport report = ReportBuilder.CreateReport(nights, "Test Report", FactionDef.NecromancerId);

		Assert.NotNull(report.ReportId);
		Assert.NotEmpty(report.ReportId);
		Assert.Equal("Test Report", report.DisplayName);
		Assert.Equal(FactionDef.NecromancerId, report.FactionId);
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
		CastleSnapshot source = new() { NightIndex = 2, Phase = BattlePhase.Combine };
		source.Buildings.Add(new BuildingSnapshot { TypeId = "Barracks", AnchorGridX = 3, AnchorGridY = 4, Health = 50 });

		CastleSnapshot clone = ReportBuilder.CloneSnapshot(source);

		Assert.Equal(source.NightIndex, clone.NightIndex);
		Assert.Equal(source.Phase, clone.Phase);
		Assert.Single(clone.Buildings);
		Assert.NotSame(source.Buildings[0], clone.Buildings[0]);
	}

	[Fact]
	public void CaptureSnapshot_RecordsGoldAndHandCards()
	{
		List<CardSnapshot> cards = new()
		{
			new CardSnapshot { BuildingType = "Barracks", Cost = 3 },
			new CardSnapshot { BuildingType = "Stable", Cost = 5 },
		};

		CastleSnapshot snapshot = ReportBuilder.CaptureSnapshot(
			new List<BuildingSnapshot>(), 1, BattlePhase.Day, _ => false, 25, cards);

		Assert.Equal(25, snapshot.Gold);
		Assert.Equal(2, snapshot.HandCards.Count);
		Assert.Equal("Barracks", snapshot.HandCards[0].BuildingType);
		Assert.Equal(3, snapshot.HandCards[0].Cost);
		Assert.Equal("Stable", snapshot.HandCards[1].BuildingType);
	}

	[Fact]
	public void CloneSnapshot_DeepCopiesHandCards()
	{
		CastleSnapshot source = new() { NightIndex = 1, Phase = BattlePhase.Day, Gold = 30 };
		source.HandCards.Add(new CardSnapshot { BuildingType = "Stable", Cost = 5 });

		CastleSnapshot clone = ReportBuilder.CloneSnapshot(source);

		Assert.Equal(30, clone.Gold);
		CardSnapshot card = Assert.Single(clone.HandCards);
		Assert.NotSame(source.HandCards[0], card);
		Assert.Equal("Stable", card.BuildingType);
		Assert.Equal(5, card.Cost);
	}
}
