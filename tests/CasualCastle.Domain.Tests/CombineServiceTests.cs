using System.Collections.Generic;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Moq;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class CombineServiceTests
{
	private static readonly GridCellOffset[] SingleCell = { new(0, 0) };

	private static Mock<IBuildingRepository> CreateRepo(bool necromancerRecipe = false)
	{
		Mock<IBuildingRepository> repo = new();
		repo.Setup(r => r.GetFootprint(It.IsAny<string>())).Returns(SingleCell);
		repo.Setup(r => r.GetCombineTier(It.IsAny<string>())).Returns(0);
		repo.Setup(r => r.IsCoreBuilding(It.IsAny<string>())).Returns(false);
		if (necromancerRecipe)
		{
			repo.Setup(r => r.GetFactionId("DeathTemple")).Returns(FactionDef.NecromancerId);
			repo.Setup(r => r.GetCombineTier("DeathTemple")).Returns(2);
			repo.Setup(r => r.GetCombineTier(It.IsAny<string>())).Returns(1);
		}
		return repo;
	}

	private static Mock<IBuildingState> CreateBuilding(string typeId, int x, int y, bool playerOwned = true)
	{
		Mock<IBuildingState> building = new();
		building.SetupGet(b => b.TypeId).Returns(typeId);
		building.SetupGet(b => b.AnchorGridX).Returns(x);
		building.SetupGet(b => b.AnchorGridY).Returns(y);
		building.SetupGet(b => b.IsDestroyed).Returns(false);
		building.SetupGet(b => b.IsManuallyPaused).Returns(false);
		building.SetupGet(b => b.IsCombineProhibited).Returns(false);
		building.SetupGet(b => b.HasEnemyOnTop).Returns(false);
		building.SetupGet(b => b.IsPlayerOwned).Returns(playerOwned);
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

	private static CombineRules CreateNecromancerRules()
	{
		CombineRules rules = new();
		rules.LoadRecipes(new List<CombineRecipe>
		{
			new() { MainTypeId = "Necropolis", MaterialTypeId = "BlightMarsh", MaterialCount = 1, ResultTypeId = "DeathTemple" }
		});
		return rules;
	}

	[Fact]
	public void TryCombineMain_ValidGroup_ReturnsResultAndRemovesInputs()
	{
		Mock<IBuildingRepository> repo = CreateRepo();
		Mock<IBuildingState> main = CreateBuilding("Barracks", 4, 4);
		Mock<IBuildingState> material = CreateBuilding("ArcheryRange", 4, 5);
		List<IBuildingState> buildings = new() { main.Object, material.Object };
		Mock<IBuildingState> result = CreateBuilding("RoyalBarracks", 4, 4);
		Mock<ICombineBuildingFactory> factory = new();
		factory.Setup(f => f.Create("RoyalBarracks", 4, 4)).Returns(result.Object);
		CombineService service = new(repo.Object, CreateRules());

		IBuildingState combined = service.TryCombineMain(buildings, main.Object, factory.Object);

		Assert.Same(result.Object, combined);
		Assert.Single(buildings);
		factory.Verify(f => f.Destroy(main.Object), Times.Once);
		factory.Verify(f => f.Destroy(material.Object), Times.Once);
	}

	[Fact]
	public void TryCombineMain_MissingMaterial_ReturnsNull()
	{
		Mock<IBuildingRepository> repo = CreateRepo();
		Mock<IBuildingState> main = CreateBuilding("Barracks", 4, 4);
		List<IBuildingState> buildings = new() { main.Object };
		Mock<ICombineBuildingFactory> factory = new();
		CombineService service = new(repo.Object, CreateRules());

		IBuildingState combined = service.TryCombineMain(buildings, main.Object, factory.Object);

		Assert.Null(combined);
		Assert.Single(buildings);
	}

	[Fact]
	public void TryCombineMain_NoRecipeForMain_ReturnsNull()
	{
		Mock<IBuildingRepository> repo = CreateRepo();
		Mock<IBuildingState> main = CreateBuilding("ArcheryRange", 4, 4);
		List<IBuildingState> buildings = new() { main.Object };
		Mock<ICombineBuildingFactory> factory = new();
		CombineService service = new(repo.Object, CreateRules());

		IBuildingState combined = service.TryCombineMain(buildings, main.Object, factory.Object);

		Assert.Null(combined);
	}

	[Fact]
	public void TryCombineMain_ProhibitedMain_ReturnsNull()
	{
		Mock<IBuildingRepository> repo = CreateRepo();
		Mock<IBuildingState> main = CreateBuilding("Barracks", 4, 4);
		main.SetupGet(b => b.IsCombineProhibited).Returns(true);
		Mock<IBuildingState> material = CreateBuilding("ArcheryRange", 4, 5);
		List<IBuildingState> buildings = new() { main.Object, material.Object };
		Mock<ICombineBuildingFactory> factory = new();
		CombineService service = new(repo.Object, CreateRules());

		IBuildingState combined = service.TryCombineMain(buildings, main.Object, factory.Object);

		Assert.Null(combined);
	}

	[Fact]
	public void TryCombineMain_ProhibitedMaterial_ReturnsNull()
	{
		Mock<IBuildingRepository> repo = CreateRepo();
		Mock<IBuildingState> main = CreateBuilding("Barracks", 4, 4);
		Mock<IBuildingState> material = CreateBuilding("ArcheryRange", 4, 5);
		material.SetupGet(b => b.IsCombineProhibited).Returns(true);
		List<IBuildingState> buildings = new() { main.Object, material.Object };
		Mock<ICombineBuildingFactory> factory = new();
		CombineService service = new(repo.Object, CreateRules());

		IBuildingState combined = service.TryCombineMain(buildings, main.Object, factory.Object);

		Assert.Null(combined);
	}

	[Fact]
	public void TryCombineMain_MainNotPlayerOwned_ReturnsNull()
	{
		Mock<IBuildingRepository> repo = CreateRepo();
		Mock<IBuildingState> main = CreateBuilding("Barracks", 4, 4, false);
		Mock<IBuildingState> material = CreateBuilding("ArcheryRange", 4, 5);
		List<IBuildingState> buildings = new() { main.Object, material.Object };
		Mock<ICombineBuildingFactory> factory = new();
		CombineService service = new(repo.Object, CreateRules());

		IBuildingState combined = service.TryCombineMain(buildings, main.Object, factory.Object);

		Assert.Null(combined);
	}

	[Fact]
	public void TryCombineMain_EnemyOnMain_ReturnsNull()
	{
		Mock<IBuildingRepository> repo = CreateRepo();
		Mock<IBuildingState> main = CreateBuilding("Barracks", 4, 4);
		main.SetupGet(b => b.HasEnemyOnTop).Returns(true);
		Mock<IBuildingState> material = CreateBuilding("ArcheryRange", 4, 5);
		List<IBuildingState> buildings = new() { main.Object, material.Object };
		Mock<ICombineBuildingFactory> factory = new();
		CombineService service = new(repo.Object, CreateRules());

		IBuildingState combined = service.TryCombineMain(buildings, main.Object, factory.Object);

		Assert.Null(combined);
	}

	[Fact]
	public void TryCombineMain_NecromancerRecipe_InsufficientCoreLevel_ReturnsNull()
	{
		Mock<IBuildingRepository> repo = CreateRepo(true);
		Mock<IBuildingState> main = CreateBuilding("Necropolis", 4, 4);
		Mock<IBuildingState> material = CreateBuilding("BlightMarsh", 4, 5);
		List<IBuildingState> buildings = new() { main.Object, material.Object };
		Mock<ICombineBuildingFactory> factory = new();
		CombineService service = new(repo.Object, CreateNecromancerRules());

		IBuildingState combined = service.TryCombineMain(buildings, main.Object, factory.Object, 2);

		Assert.Null(combined);
		Assert.Equal(2, buildings.Count);
	}

	[Fact]
	public void TryCombineMain_NecromancerRecipe_SufficientCoreLevel_ReturnsResult()
	{
		Mock<IBuildingRepository> repo = CreateRepo(true);
		Mock<IBuildingState> main = CreateBuilding("Necropolis", 4, 4);
		Mock<IBuildingState> material = CreateBuilding("BlightMarsh", 4, 5);
		List<IBuildingState> buildings = new() { main.Object, material.Object };
		Mock<IBuildingState> result = CreateBuilding("DeathTemple", 4, 4);
		Mock<ICombineBuildingFactory> factory = new();
		factory.Setup(f => f.Create("DeathTemple", 4, 4)).Returns(result.Object);
		CombineService service = new(repo.Object, CreateNecromancerRules());

		IBuildingState combined = service.TryCombineMain(buildings, main.Object, factory.Object, 3);

		Assert.Same(result.Object, combined);
		Assert.Single(buildings);
	}
}
