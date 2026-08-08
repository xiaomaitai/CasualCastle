using System.Collections.Generic;
using CasualCastle.Domain.Building;
using Moq;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class RitualCircleRulesTests
{
	[Fact]
	public void GetLevel_LevelSuffix_ReturnsLevel()
	{
		Assert.Equal(1, RitualCircleRules.GetLevel("RitualCircleL1"));
		Assert.Equal(5, RitualCircleRules.GetLevel("RitualCircleL5"));
	}

	[Fact]
	public void GetLevel_OtherType_ReturnsZero()
	{
		Assert.Equal(0, RitualCircleRules.GetLevel("Tomb"));
		Assert.Equal(0, RitualCircleRules.GetLevel(null));
	}

	[Fact]
	public void IsCoreUpgrade_AdjacentLevels_ReturnsTrue()
	{
		Assert.True(RitualCircleRules.IsCoreUpgrade("RitualCircleL1", "RitualCircleL2"));
		Assert.True(RitualCircleRules.IsCoreUpgrade("RitualCircleL4", "RitualCircleL5"));
	}

	[Fact]
	public void IsCoreUpgrade_NonAdjacent_ReturnsFalse()
	{
		Assert.False(RitualCircleRules.IsCoreUpgrade("RitualCircleL1", "RitualCircleL3"));
		Assert.False(RitualCircleRules.IsCoreUpgrade("Tomb", "Necropolis"));
	}

	[Fact]
	public void GetRequiredCoreLevel_CombineTierPlusOne()
	{
		Mock<IBuildingRepository> repo = new();
		repo.Setup(r => r.GetCombineTier("Necropolis")).Returns(1);

		Assert.Equal(2, RitualCircleRules.GetRequiredCoreLevel("Necropolis", repo.Object));
	}

	[Fact]
	public void GetCoreLevel_AliveHighest_ReturnsLevel()
	{
		Mock<IBuildingState> l1 = CreateCore("RitualCircleL1", false);
		Mock<IBuildingState> l2 = CreateCore("RitualCircleL2", false);
		Mock<IBuildingState> destroyedL3 = CreateCore("RitualCircleL3", true);

		int level = RitualCircleRules.GetCoreLevel(new List<IBuildingState> { l1.Object, destroyedL3.Object, l2.Object });

		Assert.Equal(2, level);
	}

	[Fact]
	public void GetCoreLevel_NoCore_ReturnsZero()
	{
		Mock<IBuildingState> tomb = CreateCore("Tomb", false);

		int level = RitualCircleRules.GetCoreLevel(new List<IBuildingState> { tomb.Object });

		Assert.Equal(0, level);
	}

	[Fact]
	public void CreateUpgradeRecipes_ReturnsFourEdges()
	{
		List<UpgradeRecipe> recipes = RitualCircleRules.CreateUpgradeRecipes();

		Assert.Equal(4, recipes.Count);
		Assert.Equal("RitualCircleL1", recipes[0].SourceTypeId);
		Assert.Equal("RitualCircleL2", recipes[0].ResultTypeId);
		Assert.Equal("RitualCircleL4", recipes[3].SourceTypeId);
		Assert.Equal("RitualCircleL5", recipes[3].ResultTypeId);
	}

	private static Mock<IBuildingState> CreateCore(string typeId, bool destroyed)
	{
		Mock<IBuildingState> core = new();
		core.SetupGet(c => c.TypeId).Returns(typeId);
		core.SetupGet(c => c.IsDestroyed).Returns(destroyed);
		return core;
	}
}