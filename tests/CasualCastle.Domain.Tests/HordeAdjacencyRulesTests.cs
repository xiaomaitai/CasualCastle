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

	private static Mock<IBuildingRepository> CreateRepo(GridCellOffset[] footprint)
	{
		Mock<IBuildingRepository> repo = new();
		repo.Setup(r => r.GetFootprint(It.IsAny<string>())).Returns(footprint);
		return repo;
	}

	private static Mock<IAdjacencyBuilding> CreateBuilding(string typeId, int x, int y)
	{
		Mock<IAdjacencyBuilding> building = new();
		building.SetupGet(b => b.TypeId).Returns(typeId);
		building.SetupGet(b => b.AnchorGridX).Returns(x);
		building.SetupGet(b => b.AnchorGridY).Returns(y);
		return building;
	}

	[Fact]
	public void CountAdjacentEmptyCells_AllSidesOccupied_ReturnsZero()
	{
		Mock<IBuildingRepository> repo = CreateRepo(SingleCell);
		Mock<IAdjacencyBuilding> source = CreateBuilding("OrcCamp", 5, 5);
		List<IAdjacencyBuilding> all = new()
		{
			source.Object,
			CreateBuilding("Neighbor", 5, 4).Object,
			CreateBuilding("Neighbor", 5, 6).Object,
			CreateBuilding("Neighbor", 4, 5).Object,
			CreateBuilding("Neighbor", 6, 5).Object
		};

		int count = HordeAdjacencyRules.CountAdjacentEmptyCells(source.Object, all, repo.Object);

		Assert.Equal(0, count);
	}

	[Fact]
	public void CountAdjacentEmptyCells_OneSideEmpty_ReturnsOne()
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

		int count = HordeAdjacencyRules.CountAdjacentEmptyCells(source.Object, all, repo.Object);

		Assert.Equal(1, count);
	}

	[Fact]
	public void CountAdjacentEmptyCells_TwoSidesEmpty_ReturnsTwo()
	{
		Mock<IBuildingRepository> repo = CreateRepo(SingleCell);
		Mock<IAdjacencyBuilding> source = CreateBuilding("OrcCamp", 5, 5);
		List<IAdjacencyBuilding> all = new()
		{
			source.Object,
			CreateBuilding("Neighbor", 5, 4).Object,
			CreateBuilding("Neighbor", 4, 5).Object
		};

		int count = HordeAdjacencyRules.CountAdjacentEmptyCells(source.Object, all, repo.Object);

		Assert.Equal(2, count);
	}

	[Fact]
	public void CountAdjacentEmptyCells_AllSidesEmpty_ReturnsFour()
	{
		Mock<IBuildingRepository> repo = CreateRepo(SingleCell);
		Mock<IAdjacencyBuilding> source = CreateBuilding("OrcCamp", 5, 5);
		List<IAdjacencyBuilding> all = new() { source.Object };

		int count = HordeAdjacencyRules.CountAdjacentEmptyCells(source.Object, all, repo.Object);

		Assert.Equal(4, count);
	}

	[Theory]
	[InlineData(0, 0.52f)]
	[InlineData(1, 0.64f)]
	[InlineData(2, 0.76f)]
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
	public void CalculateSpawnIntervalMultiplier_AdjacentCastle_NoBonus()
	{
		Mock<IBuildingRepository> repo = CreateRepo(SingleCell);
		Mock<IAdjacencyBuilding> source = CreateBuilding("OrcCamp", 5, 5);
		List<IAdjacencyBuilding> all = new()
		{
			source.Object,
			CreateBuilding("CastleHeart", 5, 4).Object
		};

		float mult = HordeAdjacencyRules.CalculateSpawnIntervalMultiplier(source.Object, all, repo.Object);

		Assert.Equal(0.64f, mult, 4);
	}

	[Fact]
	public void CountAdjacentEmptyCells_TwoByTwoFootprint_CountsOneCellPerSide()
	{
		Mock<IBuildingRepository> repo = CreateRepo(TwoByTwo);
		Mock<IAdjacencyBuilding> source = CreateBuilding("OrcCamp", 5, 5);
		List<IAdjacencyBuilding> all = new() { source.Object };

		int count = HordeAdjacencyRules.CountAdjacentEmptyCells(source.Object, all, repo.Object);

		Assert.Equal(4, count);
	}
}
