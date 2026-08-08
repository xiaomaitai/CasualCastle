using System.Collections.Generic;
using CasualCastle.Domain.Building;
using Moq;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class UpgradeRulesTests
{
	private static UpgradeRules CreateRules()
	{
		UpgradeRules rules = new();
		rules.LoadRecipes(new List<UpgradeRecipe>
		{
			new() { SourceTypeId = "OrcCamp", ResultTypeId = "OrcFortress" }
		});
		return rules;
	}

	private static Mock<IBuildingRepository> CreateRepo(int sourceValue, int resultValue)
	{
		Mock<IBuildingRepository> repo = new();
		repo.Setup(r => r.GetValue("OrcCamp")).Returns(sourceValue);
		repo.Setup(r => r.GetValue("OrcFortress")).Returns(resultValue);
		repo.Setup(r => r.IsCoreBuilding(It.IsAny<string>())).Returns(false);
		repo.Setup(r => r.GetFactionId(It.IsAny<string>())).Returns("alliance");
		repo.Setup(r => r.GetCombineTier("OrcFortress")).Returns(1);
		return repo;
	}

	private static UpgradeRules CreateRulesWithCoreRecipe()
	{
		UpgradeRules rules = new();
		rules.LoadRecipes(new List<UpgradeRecipe>
		{
			new() { SourceTypeId = "OrcCamp", ResultTypeId = "OrcFortress" },
			new() { SourceTypeId = "HordeCoreL1", ResultTypeId = "HordeCoreL2" },
			new() { SourceTypeId = "HordeCoreL2", ResultTypeId = "HordeCoreL3" }
		});
		return rules;
	}

	private static Mock<IBuildingState> CreateSource(bool isPlayerOwned, bool isDestroyed, string typeId = "OrcCamp")
	{
		Mock<IBuildingState> source = new();
		source.SetupGet(s => s.TypeId).Returns(typeId);
		source.SetupGet(s => s.IsDestroyed).Returns(isDestroyed);
		source.SetupGet(s => s.IsPlayerOwned).Returns(isPlayerOwned);
		return source;
	}

	[Fact]
	public void FindRecipe_ExistingSource_ReturnsRecipe()
	{
		UpgradeRules rules = CreateRules();

		UpgradeRecipe recipe = rules.FindRecipe("OrcCamp");

		Assert.NotNull(recipe);
		Assert.Equal("OrcFortress", recipe.ResultTypeId);
	}

	[Fact]
	public void FindRecipe_UnknownSource_ReturnsNull()
	{
		UpgradeRules rules = CreateRules();

		UpgradeRecipe recipe = rules.FindRecipe("MageTower");

		Assert.Null(recipe);
	}

	[Fact]
	public void CalculateCost_ValueDifference_ReturnsDifference()
	{
		UpgradeRules rules = CreateRules();
		Mock<IBuildingRepository> repo = CreateRepo(3, 7);

		int cost = rules.CalculateCost("OrcCamp", "OrcFortress", repo.Object);

		Assert.Equal(4, cost);
	}

	[Fact]
	public void CanUpgrade_PlayerOwnedAlive_ReturnsTrue()
	{
		UpgradeRules rules = CreateRules();
		Mock<IBuildingRepository> repo = CreateRepo(3, 7);
		Mock<IBuildingState> source = CreateSource(true, false);

		bool canUpgrade = rules.CanUpgrade(source.Object, repo.Object);

		Assert.True(canUpgrade);
	}

	[Fact]
	public void CanUpgrade_Destroyed_ReturnsFalse()
	{
		UpgradeRules rules = CreateRules();
		Mock<IBuildingRepository> repo = CreateRepo(3, 7);
		Mock<IBuildingState> source = CreateSource(true, true);

		bool canUpgrade = rules.CanUpgrade(source.Object, repo.Object);

		Assert.False(canUpgrade);
	}

	[Fact]
	public void CanUpgrade_EnemyOwned_ReturnsFalse()
	{
		UpgradeRules rules = CreateRules();
		Mock<IBuildingRepository> repo = CreateRepo(3, 7);
		Mock<IBuildingState> source = CreateSource(false, false);

		bool canUpgrade = rules.CanUpgrade(source.Object, repo.Object);

		Assert.False(canUpgrade);
	}

	[Fact]
	public void CanUpgrade_HordeCoreLevel_RequiresBurrowCount()
	{
		UpgradeRules rules = CreateRulesWithCoreRecipe();
		Mock<IBuildingRepository> repo = CreateRepo(3, 7);
		Mock<IBuildingState> source = CreateSource(true, false, "HordeCoreL1");

		bool withoutBurrow = rules.CanUpgrade(source.Object, repo.Object, int.MaxValue, 0);
		bool withBurrow = rules.CanUpgrade(source.Object, repo.Object, int.MaxValue, 1);

		Assert.False(withoutBurrow);
		Assert.True(withBurrow);
	}

	[Fact]
	public void CanUpgrade_HordeCoreLevelTwo_RequiresThreeBurrows()
	{
		UpgradeRules rules = CreateRulesWithCoreRecipe();
		Mock<IBuildingRepository> repo = CreateRepo(3, 7);
		Mock<IBuildingState> source = CreateSource(true, false, "HordeCoreL2");

		bool withTwoBurrows = rules.CanUpgrade(source.Object, repo.Object, int.MaxValue, 2);
		bool withThreeBurrows = rules.CanUpgrade(source.Object, repo.Object, int.MaxValue, 3);

		Assert.False(withTwoBurrows);
		Assert.True(withThreeBurrows);
	}

	[Fact]
	public void CanUpgrade_HordeBuilding_RequiresCoreLevel()
	{
		UpgradeRules rules = CreateRules();
		Mock<IBuildingRepository> repo = CreateRepo(3, 7);
		repo.Setup(r => r.GetFactionId("OrcCamp")).Returns(FactionDef.HordeId);
		Mock<IBuildingState> source = CreateSource(true, false);

		bool withLevelOneCore = rules.CanUpgrade(source.Object, repo.Object, 1, 0);
		bool withLevelTwoCore = rules.CanUpgrade(source.Object, repo.Object, 2, 0);

		Assert.False(withLevelOneCore);
		Assert.True(withLevelTwoCore);
	}

	[Fact]
	public void CanUpgrade_NewRaceHordeBuilding_RequiresCoreLevel()
	{
		UpgradeRules rules = new();
		rules.LoadRecipes(new List<UpgradeRecipe>
		{
			new() { SourceTypeId = "GoblinHut", ResultTypeId = "GoblinWorkshop" }
		});
		Mock<IBuildingRepository> repo = new();
		repo.Setup(r => r.GetValue("GoblinHut")).Returns(2);
		repo.Setup(r => r.GetValue("GoblinWorkshop")).Returns(5);
		repo.Setup(r => r.GetFactionId("GoblinHut")).Returns(FactionDef.HordeId);
		repo.Setup(r => r.GetCombineTier("GoblinWorkshop")).Returns(1);
		Mock<IBuildingState> source = CreateSource(true, false, "GoblinHut");

		bool withLevelOneCore = rules.CanUpgrade(source.Object, repo.Object, 1, 0);
		bool withLevelTwoCore = rules.CanUpgrade(source.Object, repo.Object, 2, 0);

		Assert.False(withLevelOneCore);
		Assert.True(withLevelTwoCore);
	}

	[Fact]
	public void CanUpgrade_NoRecipe_ReturnsFalse()
	{
		UpgradeRules rules = CreateRules();
		Mock<IBuildingRepository> repo = CreateRepo(3, 7);
		Mock<IBuildingState> source = CreateSource(true, false, "MageTower");

		bool canUpgrade = rules.CanUpgrade(source.Object, repo.Object);

		Assert.False(canUpgrade);
	}

	[Fact]
	public void CanUpgrade_NecromancerBuilding_RequiresRitualCircleLevel()
	{
		UpgradeRules rules = new();
		rules.LoadRecipes(new List<UpgradeRecipe>
		{
			new() { SourceTypeId = "Tomb", ResultTypeId = "Necropolis" }
		});
		Mock<IBuildingRepository> repo = new();
		repo.Setup(r => r.GetValue("Tomb")).Returns(2);
		repo.Setup(r => r.GetValue("Necropolis")).Returns(5);
		repo.Setup(r => r.GetFactionId("Tomb")).Returns(FactionDef.NecromancerId);
		repo.Setup(r => r.GetCombineTier("Necropolis")).Returns(1);
		Mock<IBuildingState> source = CreateSource(true, false, "Tomb");

		bool withLevelOneCore = rules.CanUpgrade(source.Object, repo.Object, 1, 0);
		bool withLevelTwoCore = rules.CanUpgrade(source.Object, repo.Object, 2, 0);

		Assert.False(withLevelOneCore);
		Assert.True(withLevelTwoCore);
	}

	[Fact]
	public void CanUpgrade_RitualCircleCoreUpgrade_NoCoreLevelGate()
	{
		UpgradeRules rules = new();
		rules.LoadRecipes(new List<UpgradeRecipe>
		{
			new() { SourceTypeId = "RitualCircleL1", ResultTypeId = "RitualCircleL2" }
		});
		Mock<IBuildingRepository> repo = new();
		repo.Setup(r => r.GetValue("RitualCircleL1")).Returns(0);
		repo.Setup(r => r.GetValue("RitualCircleL2")).Returns(20);
		repo.Setup(r => r.GetFactionId("RitualCircleL1")).Returns(FactionDef.NecromancerId);
		Mock<IBuildingState> source = CreateSource(true, false, "RitualCircleL1");

		bool canUpgrade = rules.CanUpgrade(source.Object, repo.Object, 0, 0);

		Assert.True(canUpgrade);
	}
}
