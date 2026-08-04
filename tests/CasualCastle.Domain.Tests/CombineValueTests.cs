using System.Collections.Generic;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Moq;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class CombineValueTests
{
	private static readonly GridCellOffset[] SingleCell = { new(0, 0) };

	private static Mock<IBuildingRepository> CreateRepo(int barracksValue, int archeryValue, int resultValue)
	{
		Mock<IBuildingRepository> repo = new();
		repo.Setup(r => r.GetValue("Barracks")).Returns(barracksValue);
		repo.Setup(r => r.GetValue("ArcheryRange")).Returns(archeryValue);
		repo.Setup(r => r.GetValue("RoyalBarracks")).Returns(resultValue);
		repo.Setup(r => r.GetFootprint(It.IsAny<string>())).Returns(SingleCell);
		repo.Setup(r => r.GetCombineTier(It.IsAny<string>())).Returns(0);
		repo.Setup(r => r.IsCoreBuilding(It.IsAny<string>())).Returns(false);
		return repo;
	}

	private static Mock<IBuildingState> CreateBuilding(string typeId, int x, int y)
	{
		Mock<IBuildingState> building = new();
		building.SetupGet(b => b.TypeId).Returns(typeId);
		building.SetupGet(b => b.AnchorGridX).Returns(x);
		building.SetupGet(b => b.AnchorGridY).Returns(y);
		building.SetupGet(b => b.IsDestroyed).Returns(false);
		building.SetupGet(b => b.IsManuallyPaused).Returns(false);
		building.SetupGet(b => b.IsCombineProhibited).Returns(false);
		building.SetupGet(b => b.HasEnemyOnTop).Returns(false);
		building.SetupGet(b => b.IsPlayerOwned).Returns(true);
		building.SetupGet(b => b.ContributesToAdjacency).Returns(true);
		return building;
	}

	private static CombineRules CreateRules()
	{
		CombineRules rules = new();
		rules.LoadRecipes(new List<CombineRecipe>
		{
			new() { MainTypeId = "Barracks", MaterialTypeId = "ArcheryRange", MaterialCount = 1, ResultTypeId = "RoyalBarracks" }
		});
		return rules;
	}

	[Fact]
	public void ResolveCombines_ResultValueUsesConfiguredValue_NotSumOfInputValues()
	{
		Mock<IBuildingRepository> repo = CreateRepo(3, 3, 7);
		Mock<IBuildingState> main = CreateBuilding("Barracks", 4, 4);
		Mock<IBuildingState> material = CreateBuilding("ArcheryRange", 4, 5);
		List<IBuildingState> buildings = new() { main.Object, material.Object };
		Mock<IBuildingState> result = CreateBuilding("RoyalBarracks", 4, 4);
		Mock<ICombineBuildingFactory> factory = new();
		factory.Setup(f => f.Create("RoyalBarracks", 4, 4)).Returns(result.Object);
		CombineService service = new(repo.Object, CreateRules());
		IBuildingState combined = null;

		service.ResolveCombines(buildings, true, true, true, factory.Object, r => combined = r);

		Assert.Same(result.Object, combined);
		Assert.Equal(7, repo.Object.GetValue(combined.TypeId));
		Assert.NotEqual(3 + 3, repo.Object.GetValue(combined.TypeId));
		factory.Verify(f => f.Destroy(main.Object), Times.Once);
		factory.Verify(f => f.Destroy(material.Object), Times.Once);
		Assert.Single(buildings);
	}

	[Fact]
	public void ResolveCombines_ResultValueStaysConfigured_WhenInputValuesDiffer()
	{
		Mock<IBuildingRepository> repo = CreateRepo(5, 2, 9);
		Mock<IBuildingState> main = CreateBuilding("Barracks", 4, 4);
		Mock<IBuildingState> material = CreateBuilding("ArcheryRange", 4, 5);
		List<IBuildingState> buildings = new() { main.Object, material.Object };
		Mock<IBuildingState> result = CreateBuilding("RoyalBarracks", 4, 4);
		Mock<ICombineBuildingFactory> factory = new();
		factory.Setup(f => f.Create("RoyalBarracks", 4, 4)).Returns(result.Object);
		CombineService service = new(repo.Object, CreateRules());
		IBuildingState combined = null;

		service.ResolveCombines(buildings, true, true, true, factory.Object, r => combined = r);

		Assert.Same(result.Object, combined);
		Assert.Equal(9, repo.Object.GetValue(combined.TypeId));
		Assert.NotEqual(5 + 2, repo.Object.GetValue(combined.TypeId));
	}
}
