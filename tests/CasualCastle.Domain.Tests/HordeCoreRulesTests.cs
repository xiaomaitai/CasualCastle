using System.Collections.Generic;
using CasualCastle.Domain.Building;
using Moq;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class HordeCoreRulesTests
{
	[Theory]
	[InlineData("HordeCoreL1", 1)]
	[InlineData("HordeCoreL5", 5)]
	[InlineData("OrcCamp", 0)]
	[InlineData("Burrow", 0)]
	[InlineData("HordeCoreLX", 0)]
	public void GetLevel_ParsesLevelSuffix(string typeId, int expected)
	{
		Assert.Equal(expected, HordeCoreRules.GetLevel(typeId));
	}

	[Fact]
	public void IsBurrow_MatchesBurrowType()
	{
		Assert.True(HordeCoreRules.IsBurrow(HordeCoreRules.BurrowTypeId));
		Assert.False(HordeCoreRules.IsBurrow("OrcCamp"));
	}

	[Theory]
	[InlineData(1, 1)]
	[InlineData(2, 3)]
	[InlineData(3, 10)]
	[InlineData(4, 30)]
	public void GetUpgradeBurrowRequirement_ReturnsExpected(int level, int expected)
	{
		Assert.Equal(expected, HordeCoreRules.GetUpgradeBurrowRequirement(level));
	}

	[Fact]
	public void GetUpgradeBurrowRequirement_MaxLevel_ReturnsMaxValue()
	{
		Assert.Equal(int.MaxValue, HordeCoreRules.GetUpgradeBurrowRequirement(5));
	}

	[Fact]
	public void IsCoreUpgrade_ConsecutiveLevels_ReturnsTrue()
	{
		Assert.True(HordeCoreRules.IsCoreUpgrade("HordeCoreL2", "HordeCoreL3"));
		Assert.False(HordeCoreRules.IsCoreUpgrade("OrcCamp", "OrcFortress"));
		Assert.False(HordeCoreRules.IsCoreUpgrade("HordeCoreL1", "HordeCoreL3"));
	}

	[Fact]
	public void CreateUpgradeRecipes_ReturnsFourConsecutiveRecipes()
	{
		List<UpgradeRecipe> recipes = HordeCoreRules.CreateUpgradeRecipes();

		Assert.Equal(4, recipes.Count);
		Assert.Equal("HordeCoreL1", recipes[0].SourceTypeId);
		Assert.Equal("HordeCoreL2", recipes[0].ResultTypeId);
		Assert.Equal("HordeCoreL4", recipes[3].SourceTypeId);
		Assert.Equal("HordeCoreL5", recipes[3].ResultTypeId);
	}

	[Fact]
	public void CountBurrows_IgnoresDestroyedAndNonBurrow()
	{
		Mock<IBuildingState> burrow = new();
		burrow.SetupGet(b => b.TypeId).Returns(HordeCoreRules.BurrowTypeId);
		burrow.SetupGet(b => b.IsDestroyed).Returns(false);
		Mock<IBuildingState> deadBurrow = new();
		deadBurrow.SetupGet(b => b.TypeId).Returns(HordeCoreRules.BurrowTypeId);
		deadBurrow.SetupGet(b => b.IsDestroyed).Returns(true);
		Mock<IBuildingState> camp = new();
		camp.SetupGet(b => b.TypeId).Returns("OrcCamp");
		camp.SetupGet(b => b.IsDestroyed).Returns(false);

		int count = HordeCoreRules.CountBurrows(new List<IBuildingState> { burrow.Object, deadBurrow.Object, camp.Object });

		Assert.Equal(1, count);
	}

	[Fact]
	public void GetCoreLevel_ReturnsHighestAliveLevel()
	{
		Mock<IBuildingState> levelTwo = new();
		levelTwo.SetupGet(b => b.TypeId).Returns("HordeCoreL2");
		levelTwo.SetupGet(b => b.IsDestroyed).Returns(false);
		Mock<IBuildingState> levelFive = new();
		levelFive.SetupGet(b => b.TypeId).Returns("HordeCoreL5");
		levelFive.SetupGet(b => b.IsDestroyed).Returns(false);
		Mock<IBuildingState> deadLevelFour = new();
		deadLevelFour.SetupGet(b => b.TypeId).Returns("HordeCoreL4");
		deadLevelFour.SetupGet(b => b.IsDestroyed).Returns(true);

		int level = HordeCoreRules.GetCoreLevel(new List<IBuildingState> { levelTwo.Object, levelFive.Object, deadLevelFour.Object });

		Assert.Equal(5, level);
	}
}
