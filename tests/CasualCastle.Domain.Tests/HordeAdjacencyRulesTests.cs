using System.Collections.Generic;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Moq;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class HordeAdjacencyRulesTests
{
	private static readonly GridCellOffset[] SingleCell = { new(0, 0) };
	private static readonly GridCellOffset[] TwoByTwo =
	{
		new(0, 0),
		new(1, 0),
		new(0, 1),
		new(1, 1)
	};

	private static Mock<IBuildingRepository> CreateRepo(
		GridCellOffset[] footprint, Dictionary<string, string> raceById = null)
	{
		Mock<IBuildingRepository> repo = new();
		repo.Setup(r => r.GetFootprint(It.IsAny<string>())).Returns(footprint);
		repo.Setup(r => r.GetRaceId(It.IsAny<string>()))
			.Returns((string typeId) => raceById != null && raceById.TryGetValue(typeId, out string race) ? race : "");
		return repo;
	}

	private static Mock<IAdjacencyBuilding> CreateBuilding(string typeId, int x, int y, bool contributes = true)
	{
		Mock<IAdjacencyBuilding> building = new();
		building.SetupGet(b => b.TypeId).Returns(typeId);
		building.SetupGet(b => b.AnchorGridX).Returns(x);
		building.SetupGet(b => b.AnchorGridY).Returns(y);
		building.SetupGet(b => b.ContributesToAdjacency).Returns(contributes);
		return building;
	}

	[Fact]
	public void CountAdjacencyBonusCells_AllSidesDifferentRace_ReturnsZero()
	{
		Dictionary<string, string> races = new() { ["OrcCamp"] = "Orc" };
		Mock<IBuildingRepository> repo = CreateRepo(SingleCell, races);
		Mock<IAdjacencyBuilding> source = CreateBuilding("OrcCamp", 5, 5);
		List<IAdjacencyBuilding> all = new()
		{
			source.Object,
			CreateBuilding("GoblinHut", 5, 4).Object,
			CreateBuilding("GoblinHut", 5, 6).Object,
			CreateBuilding("GoblinHut", 4, 5).Object,
			CreateBuilding("GoblinHut", 6, 5).Object
		};

		int count = HordeAdjacencyRules.CountAdjacencyBonusCells(source.Object, all, repo.Object);

		Assert.Equal(0, count);
	}

	[Fact]
	public void CountAdjacencyBonusCells_OneSideEmpty_ReturnsOne()
	{
		Mock<IBuildingRepository> repo = CreateRepo(SingleCell);
		Mock<IAdjacencyBuilding> source = CreateBuilding("OrcCamp", 5, 5);
		List<IAdjacencyBuilding> all = new()
		{
			source.Object,
			CreateBuilding("Neighbor", 5, 4).Object,
			CreateBuilding("Neighbor", 5, 6).Object,
			CreateBuilding("Neighbor", 4, 5).Object
		};

		int count = HordeAdjacencyRules.CountAdjacencyBonusCells(source.Object, all, repo.Object);

		Assert.Equal(1, count);
	}

	[Fact]
	public void CountAdjacencyBonusCells_SameRaceNeighbor_CountsAsBonus()
	{
		Dictionary<string, string> races = new() { ["OrcCamp"] = "Orc", ["OrcFortress"] = "Orc" };
		Mock<IBuildingRepository> repo = CreateRepo(SingleCell, races);
		Mock<IAdjacencyBuilding> source = CreateBuilding("OrcCamp", 5, 5);
		List<IAdjacencyBuilding> all = new()
		{
			source.Object,
			CreateBuilding("OrcFortress", 5, 4).Object,
			CreateBuilding("Neighbor", 5, 6).Object,
			CreateBuilding("Neighbor", 4, 5).Object
		};

		int count = HordeAdjacencyRules.CountAdjacencyBonusCells(source.Object, all, repo.Object);

		Assert.Equal(2, count);
	}

	[Fact]
	public void CountAdjacencyBonusCells_NewRaceT1AndT2_SameRaceCountsAsBonus()
	{
		Dictionary<string, string> races = new() { ["GoblinHut"] = "Goblin", ["GoblinWorkshop"] = "Goblin" };
		Mock<IBuildingRepository> repo = CreateRepo(SingleCell, races);
		Mock<IAdjacencyBuilding> source = CreateBuilding("GoblinHut", 5, 5);
		List<IAdjacencyBuilding> all = new()
		{
			source.Object,
			CreateBuilding("GoblinWorkshop", 5, 4).Object,
			CreateBuilding("Neighbor", 5, 6).Object,
			CreateBuilding("Neighbor", 4, 5).Object
		};

		int count = HordeAdjacencyRules.CountAdjacencyBonusCells(source.Object, all, repo.Object);

		Assert.Equal(2, count);
	}

	[Fact]
	public void CountAdjacencyBonusCells_BurrowNeighbor_CountsAsBonus()
	{
		Mock<IBuildingRepository> repo = CreateRepo(SingleCell);
		Mock<IAdjacencyBuilding> source = CreateBuilding("OrcCamp", 5, 5);
		List<IAdjacencyBuilding> all = new()
		{
			source.Object,
			CreateBuilding(HordeCoreRules.BurrowTypeId, 5, 4).Object
		};

		int count = HordeAdjacencyRules.CountAdjacencyBonusCells(source.Object, all, repo.Object);

		Assert.Equal(4, count);
	}

	[Fact]
	public void CountAdjacencyBonusCells_DestroyedNeighbor_CountsAsEmpty()
	{
		Mock<IBuildingRepository> repo = CreateRepo(SingleCell);
		Mock<IAdjacencyBuilding> source = CreateBuilding("OrcCamp", 5, 5);
		List<IAdjacencyBuilding> all = new()
		{
			source.Object,
			CreateBuilding("Neighbor", 5, 4, false).Object
		};

		int count = HordeAdjacencyRules.CountAdjacencyBonusCells(source.Object, all, repo.Object);

		Assert.Equal(4, count);
	}

	[Fact]
	public void CountAdjacencyBonusCells_AllSidesEmpty_ReturnsFour()
	{
		Mock<IBuildingRepository> repo = CreateRepo(SingleCell);
		Mock<IAdjacencyBuilding> source = CreateBuilding("OrcCamp", 5, 5);
		List<IAdjacencyBuilding> all = new() { source.Object };

		int count = HordeAdjacencyRules.CountAdjacencyBonusCells(source.Object, all, repo.Object);

		Assert.Equal(4, count);
	}

	[Fact]
	public void CountAdjacencyBonusCells_TwoByTwoFootprint_CountsOneCellPerSide()
	{
		Mock<IBuildingRepository> repo = CreateRepo(TwoByTwo);
		Mock<IAdjacencyBuilding> source = CreateBuilding("OrcCamp", 5, 5);
		List<IAdjacencyBuilding> all = new() { source.Object };

		int count = HordeAdjacencyRules.CountAdjacencyBonusCells(source.Object, all, repo.Object);

		Assert.Equal(4, count);
	}

	[Theory]
	[InlineData(0, 0.80f)]
	[InlineData(1, 0.85f)]
	[InlineData(2, 0.90f)]
	[InlineData(4, 1.00f)]
	public void CalculateSpawnIntervalMultiplier_EmptyCellCount_ReturnsExpectedMultiplier(int occupiedCount, float expected)
	{
		Mock<IBuildingRepository> repo = CreateRepo(SingleCell);
		Mock<IAdjacencyBuilding> source = CreateBuilding("OrcCamp", 5, 5);
		List<IAdjacencyBuilding> all = new() { source.Object };
		(int x, int y)[] sides = { (5, 4), (5, 6), (4, 5), (6, 5) };
		for (int i = 0; i < occupiedCount; i++)
			all.Add(CreateBuilding("Neighbor", sides[i].x, sides[i].y).Object);

		float mult = HordeAdjacencyRules.CalculateSpawnIntervalMultiplier(source.Object, all, repo.Object);

		Assert.Equal(expected, mult, 4);
	}

	[Fact]
	public void CalculateSpawnIntervalMultiplier_BurrowIncreasesRate()
	{
		Mock<IBuildingRepository> repo = CreateRepo(SingleCell);
		Mock<IAdjacencyBuilding> source = CreateBuilding("OrcCamp", 5, 5);
		List<IAdjacencyBuilding> all = new()
		{
			source.Object,
			CreateBuilding(HordeCoreRules.BurrowTypeId, 0, 0).Object
		};

		float mult = HordeAdjacencyRules.CalculateSpawnIntervalMultiplier(source.Object, all, repo.Object);

		Assert.Equal(1f - 4f * 0.06f, mult, 4);
	}

	[Fact]
	public void CalculateSpawnIntervalMultiplier_TwoBurrowsIncreasesRateTwice()
	{
		Mock<IBuildingRepository> repo = CreateRepo(SingleCell);
		Mock<IAdjacencyBuilding> source = CreateBuilding("OrcCamp", 5, 5);
		List<IAdjacencyBuilding> all = new()
		{
			source.Object,
			CreateBuilding(HordeCoreRules.BurrowTypeId, 0, 0).Object,
			CreateBuilding(HordeCoreRules.BurrowTypeId, 0, 2).Object
		};

		float mult = HordeAdjacencyRules.CalculateSpawnIntervalMultiplier(source.Object, all, repo.Object);

		Assert.Equal(1f - 4f * 0.07f, mult, 4);
	}

	[Fact]
	public void CalculateSpawnIntervalMultiplier_SameRaceNeighbor_KeepsBonus()
	{
		Dictionary<string, string> races = new() { ["OrcCamp"] = "Orc", ["OrcFortress"] = "Orc" };
		Mock<IBuildingRepository> repo = CreateRepo(SingleCell, races);
		Mock<IAdjacencyBuilding> source = CreateBuilding("OrcCamp", 5, 5);
		List<IAdjacencyBuilding> all = new()
		{
			source.Object,
			CreateBuilding("OrcFortress", 5, 4).Object
		};

		float mult = HordeAdjacencyRules.CalculateSpawnIntervalMultiplier(source.Object, all, repo.Object);

		Assert.Equal(1f - 4f * 0.05f, mult, 4);
	}

	[Fact]
	public void CalculateSpawnIntervalMultiplier_AdjacentDifferentRace_NoBonus()
	{
		Dictionary<string, string> races = new() { ["OrcCamp"] = "Orc", ["GoblinHut"] = "Goblin" };
		Mock<IBuildingRepository> repo = CreateRepo(SingleCell, races);
		Mock<IAdjacencyBuilding> source = CreateBuilding("OrcCamp", 5, 5);
		List<IAdjacencyBuilding> all = new()
		{
			source.Object,
			CreateBuilding("GoblinHut", 5, 4).Object
		};

		float mult = HordeAdjacencyRules.CalculateSpawnIntervalMultiplier(source.Object, all, repo.Object);

		Assert.Equal(1f - 3f * 0.05f, mult, 4);
	}

	[Fact]
	public void CountBurrows_CountsOnlyAliveBurrows()
	{
		Mock<IBuildingRepository> repo = CreateRepo(SingleCell);
		List<IAdjacencyBuilding> all = new()
		{
			CreateBuilding(HordeCoreRules.BurrowTypeId, 0, 0).Object,
			CreateBuilding(HordeCoreRules.BurrowTypeId, 0, 2, false).Object,
			CreateBuilding("OrcCamp", 2, 2).Object
		};

		int count = HordeAdjacencyRules.CountBurrows(all);

		Assert.Equal(1, count);
	}
}
