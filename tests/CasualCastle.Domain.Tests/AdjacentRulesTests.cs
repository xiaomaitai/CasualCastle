using System.Collections.Generic;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Moq;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class AdjacentRulesTests
{
	private Mock<IBuildingRepository> CreateRepo(string typeId, GridCellOffset[] footprint)
	{
		Mock<IBuildingRepository> repo = new();
		repo.Setup(r => r.GetFootprint(typeId)).Returns(footprint);
		return repo;
	}

	[Fact]
	public void GetAdjacentBuildings_OrthogonalNeighbor_ReturnsNeighbor()
	{
		GridCellOffset[] singleCell = new[] { new GridCellOffset(0, 0) };
		Mock<IBuildingRepository> repo = CreateRepo("TestType", singleCell);
		Mock<IAdjacencyBuilding> source = new();
		source.SetupGet(s => s.TypeId).Returns("TestType");
		source.SetupGet(s => s.AnchorGridX).Returns(5);
		source.SetupGet(s => s.AnchorGridY).Returns(5);

		Mock<IAdjacencyBuilding> neighbor = new();
		neighbor.SetupGet(n => n.TypeId).Returns("TestType");
		neighbor.SetupGet(n => n.AnchorGridX).Returns(6);
		neighbor.SetupGet(n => n.AnchorGridY).Returns(5);

		List<IAdjacencyBuilding> all = new() { source.Object, neighbor.Object };

		HashSet<IAdjacencyBuilding> result = AdjacentRules.GetAdjacentBuildings(source.Object, all, repo.Object);

		Assert.Contains(neighbor.Object, result);
	}

	[Fact]
	public void GetAdjacentBuildings_NotAdjacent_DoesNotReturnBuilding()
	{
		GridCellOffset[] singleCell = new[] { new GridCellOffset(0, 0) };
		Mock<IBuildingRepository> repo = CreateRepo("TestType", singleCell);
		Mock<IAdjacencyBuilding> source = new();
		source.SetupGet(s => s.TypeId).Returns("TestType");
		source.SetupGet(s => s.AnchorGridX).Returns(5);
		source.SetupGet(s => s.AnchorGridY).Returns(5);

		Mock<IAdjacencyBuilding> far = new();
		far.SetupGet(n => n.TypeId).Returns("TestType");
		far.SetupGet(n => n.AnchorGridX).Returns(10);
		far.SetupGet(n => n.AnchorGridY).Returns(10);

		List<IAdjacencyBuilding> all = new() { source.Object, far.Object };

		HashSet<IAdjacencyBuilding> result = AdjacentRules.GetAdjacentBuildings(source.Object, all, repo.Object);

		Assert.DoesNotContain(far.Object, result);
	}

	[Fact]
	public void CountAdjacentOfType_CountsMatchingNeighbors()
	{
		GridCellOffset[] singleCell = new[] { new GridCellOffset(0, 0) };
		Mock<IBuildingRepository> repo = CreateRepo("Barracks", singleCell);
		Mock<IAdjacencyBuilding> source = new();
		source.SetupGet(s => s.TypeId).Returns("Barracks");
		source.SetupGet(s => s.AnchorGridX).Returns(5);
		source.SetupGet(s => s.AnchorGridY).Returns(5);
		source.SetupGet(s => s.ContributesToAdjacency).Returns(true);

		Mock<IAdjacencyBuilding> neighbor = new();
		neighbor.SetupGet(n => n.TypeId).Returns("Barracks");
		neighbor.SetupGet(n => n.AnchorGridX).Returns(6);
		neighbor.SetupGet(n => n.AnchorGridY).Returns(5);
		neighbor.SetupGet(n => n.ContributesToAdjacency).Returns(true);

		List<IAdjacencyBuilding> all = new() { source.Object, neighbor.Object };

		int count = AdjacentRules.CountAdjacentOfType(source.Object, all, "Barracks", repo.Object);

		Assert.Equal(1, count);
	}

	[Fact]
	public void CalculateWorkSpeedMultiplier_NoAdjacent_ReturnsOne()
	{
		GridCellOffset[] singleCell = new[] { new GridCellOffset(0, 0) };
		Mock<IBuildingRepository> repo = CreateRepo("TestType", singleCell);
		Mock<IAdjacencyBuilding> source = new();
		source.SetupGet(s => s.TypeId).Returns("TestType");
		source.SetupGet(s => s.AnchorGridX).Returns(5);
		source.SetupGet(s => s.AnchorGridY).Returns(5);
		source.SetupGet(s => s.ContributesToAdjacency).Returns(true);

		List<IAdjacencyBuilding> all = new() { source.Object };

		float mult = AdjacentRules.CalculateWorkSpeedMultiplier(source.Object, all, repo.Object, (a, b) => a == b);

		Assert.Equal(1f, mult);
	}

	[Fact]
	public void CalculateWorkSpeedMultiplier_OneAdjacentSameLine_ReturnsBoostedMultiplier()
	{
		GridCellOffset[] singleCell = new[] { new GridCellOffset(0, 0) };
		Mock<IBuildingRepository> repo = CreateRepo("TestType", singleCell);
		Mock<IAdjacencyBuilding> source = new();
		source.SetupGet(s => s.TypeId).Returns("TestType");
		source.SetupGet(s => s.AnchorGridX).Returns(5);
		source.SetupGet(s => s.AnchorGridY).Returns(5);
		source.SetupGet(s => s.ContributesToAdjacency).Returns(true);

		Mock<IAdjacencyBuilding> neighbor = new();
		neighbor.SetupGet(n => n.TypeId).Returns("TestType");
		neighbor.SetupGet(n => n.AnchorGridX).Returns(6);
		neighbor.SetupGet(n => n.AnchorGridY).Returns(5);
		neighbor.SetupGet(n => n.ContributesToAdjacency).Returns(true);

		List<IAdjacencyBuilding> all = new() { source.Object, neighbor.Object };

		float mult = AdjacentRules.CalculateWorkSpeedMultiplier(source.Object, all, repo.Object, (a, b) => a == b);

		float expected = 1f / (float)System.Math.Pow(0.85, 1);
		Assert.Equal(expected, mult, 4);
	}
}
