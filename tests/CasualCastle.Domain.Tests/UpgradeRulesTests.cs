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
		return repo;
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
	public void CanUpgrade_CoreBuilding_ReturnsFalse()
	{
		UpgradeRules rules = CreateRules();
		Mock<IBuildingRepository> repo = CreateRepo(3, 7);
		repo.Setup(r => r.IsCoreBuilding(It.IsAny<string>())).Returns(true);
		Mock<IBuildingState> source = CreateSource(true, false);

		bool canUpgrade = rules.CanUpgrade(source.Object, repo.Object);

		Assert.False(canUpgrade);
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
}
