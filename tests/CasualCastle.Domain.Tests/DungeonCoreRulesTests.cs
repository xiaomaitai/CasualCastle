using System.Collections.Generic;
using CasualCastle.Domain.Building;
using Moq;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class DungeonCoreRulesTests
{
	[Theory]
	[InlineData("DungeonThroneL1", 1)]
	[InlineData("DungeonThroneL5", 5)]
	[InlineData("DemonGate", 0)]
	[InlineData("DungeonThroneLX", 0)]
	public void GetLevel_ParsesLevelSuffix(string typeId, int expected)
	{
		Assert.Equal(expected, DungeonCoreRules.GetLevel(typeId));
	}

	[Fact]
	public void IsCoreUpgrade_ConsecutiveLevels_ReturnsTrue()
	{
		Assert.True(DungeonCoreRules.IsCoreUpgrade("DungeonThroneL2", "DungeonThroneL3"));
		Assert.False(DungeonCoreRules.IsCoreUpgrade("DemonGate", "HellRift"));
		Assert.False(DungeonCoreRules.IsCoreUpgrade("DungeonThroneL1", "DungeonThroneL3"));
	}

	[Fact]
	public void CreateUpgradeRecipes_ReturnsFourConsecutiveRecipes()
	{
		List<UpgradeRecipe> recipes = DungeonCoreRules.CreateUpgradeRecipes();

		Assert.Equal(4, recipes.Count);
		Assert.Equal("DungeonThroneL1", recipes[0].SourceTypeId);
		Assert.Equal("DungeonThroneL2", recipes[0].ResultTypeId);
		Assert.Equal("DungeonThroneL4", recipes[3].SourceTypeId);
		Assert.Equal("DungeonThroneL5", recipes[3].ResultTypeId);
	}

	[Fact]
	public void GetCoreLevel_ReturnsHighestAliveLevel()
	{
		Mock<IBuildingState> levelTwo = new();
		levelTwo.SetupGet(b => b.TypeId).Returns("DungeonThroneL2");
		levelTwo.SetupGet(b => b.IsDestroyed).Returns(false);
		Mock<IBuildingState> levelFive = new();
		levelFive.SetupGet(b => b.TypeId).Returns("DungeonThroneL5");
		levelFive.SetupGet(b => b.IsDestroyed).Returns(false);
		Mock<IBuildingState> deadLevelFour = new();
		deadLevelFour.SetupGet(b => b.TypeId).Returns("DungeonThroneL4");
		deadLevelFour.SetupGet(b => b.IsDestroyed).Returns(true);

		int level = DungeonCoreRules.GetCoreLevel(new List<IBuildingState> { levelTwo.Object, levelFive.Object, deadLevelFour.Object });

		Assert.Equal(5, level);
	}

	[Fact]
	public void GetRequiredCoreLevel_UsesCombineTierPlusOne()
	{
		Mock<IBuildingRepository> repo = new();
		repo.Setup(r => r.GetCombineTier("AbyssAltar")).Returns(2);

		Assert.Equal(3, DungeonCoreRules.GetRequiredCoreLevel("AbyssAltar", repo.Object));
	}
}