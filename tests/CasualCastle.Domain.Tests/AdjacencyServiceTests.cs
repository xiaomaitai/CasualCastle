using System.Collections.Generic;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Moq;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class AdjacencyServiceTests
{
	private static readonly GridCellOffset[] SingleCell = { new(0, 0) };

	private static Mock<IBuildingRepository> CreateRepo(string factionId)
	{
		Mock<IBuildingRepository> repo = new();
		repo.Setup(r => r.GetFootprint(It.IsAny<string>())).Returns(SingleCell);
		repo.Setup(r => r.GetFactionId(It.IsAny<string>())).Returns(factionId);
		repo.Setup(r => r.GetRaceId(It.IsAny<string>())).Returns("");
		return repo;
	}

	private static Mock<IBuildingState> CreateBuilding(int x, int y, string typeId = "TestBuilding")
	{
		Mock<IBuildingState> building = new();
		building.SetupGet(b => b.TypeId).Returns(typeId);
		building.SetupGet(b => b.AnchorGridX).Returns(x);
		building.SetupGet(b => b.AnchorGridY).Returns(y);
		building.SetupGet(b => b.ContributesToAdjacency).Returns(true);
		return building;
	}

	[Fact]
	public void RefreshCastle_HordeBuildingFourEmptySides_SetsIntervalBonusMultiplier()
	{
		Mock<IBuildingRepository> repo = CreateRepo(FactionDef.HordeId);
		Mock<IBuildingState> building = CreateBuilding(5, 5);
		AdjacencyService service = new(repo.Object, new CombineRules());
		float expected = 1f / 0.80f;

		service.RefreshCastle(new List<IBuildingState> { building.Object });

		building.Verify(b => b.SetWorkSpeedMultiplier(It.Is<float>(v => System.Math.Abs(v - expected) < 0.0001f)), Times.Once);
	}

	[Fact]
	public void RefreshCastle_HordeBuildingAdjacentOccupied_ReducesBonus()
	{
		Mock<IBuildingRepository> repo = CreateRepo(FactionDef.HordeId);
		Mock<IBuildingState> building = CreateBuilding(5, 5);
		Mock<IBuildingState> neighbor = CreateBuilding(5, 4);
		AdjacencyService service = new(repo.Object, new CombineRules());
		float expected = 1f / 0.85f;

		service.RefreshCastle(new List<IBuildingState> { building.Object, neighbor.Object });

		building.Verify(b => b.SetWorkSpeedMultiplier(It.Is<float>(v => System.Math.Abs(v - expected) < 0.0001f)), Times.Once);
	}

	[Fact]
	public void RefreshCastle_HordeBuildingFullySurrounded_NoBonus()
	{
		Mock<IBuildingRepository> repo = CreateRepo(FactionDef.HordeId);
		Mock<IBuildingState> building = CreateBuilding(5, 5);
		List<IBuildingState> all = new() { building.Object };
		(int x, int y)[] sides = { (5, 4), (5, 6), (4, 5), (6, 5) };
		foreach ((int x, int y) side in sides)
			all.Add(CreateBuilding(side.x, side.y).Object);
		AdjacencyService service = new(repo.Object, new CombineRules());

		service.RefreshCastle(all);

		building.Verify(b => b.SetWorkSpeedMultiplier(It.Is<float>(v => System.Math.Abs(v - 1f) < 0.0001f)), Times.Once);
	}

	[Fact]
	public void RefreshCastle_HordeBuildingWithBurrow_RateIncreases()
	{
		Mock<IBuildingRepository> repo = CreateRepo(FactionDef.HordeId);
		Mock<IBuildingState> building = CreateBuilding(5, 5);
		Mock<IBuildingState> burrow = CreateBuilding(0, 0, HordeCoreRules.BurrowTypeId);
		AdjacencyService service = new(repo.Object, new CombineRules());
		float expected = 1f / 0.76f;

		service.RefreshCastle(new List<IBuildingState> { building.Object, burrow.Object });

		building.Verify(b => b.SetWorkSpeedMultiplier(It.Is<float>(v => System.Math.Abs(v - expected) < 0.0001f)), Times.Once);
	}

	[Fact]
	public void RefreshCastle_AllianceBuildingNoSameLineNeighbor_KeepsDefaultMultiplier()
	{
		Mock<IBuildingRepository> repo = CreateRepo("alliance");
		Mock<IBuildingState> building = CreateBuilding(5, 5);
		AdjacencyService service = new(repo.Object, new CombineRules());

		service.RefreshCastle(new List<IBuildingState> { building.Object });

		building.Verify(b => b.SetWorkSpeedMultiplier(1f), Times.Once);
	}

	[Fact]
	public void RefreshCastle_AllianceBuildingAdjacentSameLine_AppliesBoost()
	{
		Mock<IBuildingRepository> repo = CreateRepo("alliance");
		Mock<IBuildingState> building = CreateBuilding(5, 5);
		Mock<IBuildingState> neighbor = CreateBuilding(6, 5);
		CombineRules rules = new();
		rules.LoadRecipes(new List<CombineRecipe>
		{
			new() { MainTypeId = "TestBuilding", MaterialTypeId = "TestBuilding", MaterialCount = 1, ResultTypeId = "TestBuilding" }
		});
		AdjacencyService service = new(repo.Object, rules);
		float expected = 1f / (float)System.Math.Pow(0.85, 1);

		service.RefreshCastle(new List<IBuildingState> { building.Object, neighbor.Object });

		building.Verify(b => b.SetWorkSpeedMultiplier(It.Is<float>(v => System.Math.Abs(v - expected) < 0.0001f)), Times.Once);
	}
}
