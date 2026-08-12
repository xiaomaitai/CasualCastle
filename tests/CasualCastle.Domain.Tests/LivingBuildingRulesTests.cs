using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class LivingBuildingRulesTests
{
	[Theory]
	[InlineData("FleshNest", true)]
	[InlineData("PusPool", true)]
	[InlineData("NurgleGarden", true)]
	[InlineData("DemonGate", false)]
	public void IsLivingBuilding_MatchesNurgleLine(string typeId, bool expected)
	{
		Assert.Equal(expected, LivingBuildingRules.IsLivingBuilding(typeId));
	}

	[Theory]
	[InlineData("FleshNest", "PusPool")]
	[InlineData("PusPool", "RegrowthTree")]
	[InlineData("NurgleGarden", null)]
	[InlineData("DemonGate", null)]
	public void GetNextInLine_ReturnsNextTier(string typeId, string expected)
	{
		Assert.Equal(expected, LivingBuildingRules.GetNextInLine(typeId));
	}

	[Theory]
	[InlineData("FleshNest", 2)]
	[InlineData("PusPool", 3)]
	[InlineData("RegrowthTree", 4)]
	[InlineData("PlagueMaw", 5)]
	public void GetRequiredKingLevel_ScalesWithTier(string typeId, int expected)
	{
		Assert.Equal(expected, LivingBuildingRules.GetRequiredKingLevel(typeId));
	}

	[Fact]
	public void GetGrowthInterval_ShrinksWithKingLevel()
	{
		float low = LivingBuildingRules.GetGrowthInterval("FleshNest", 1);
		float high = LivingBuildingRules.GetGrowthInterval("FleshNest", 5);

		Assert.Equal(LivingBuildingRules.BaseGrowthIntervalSeconds, low);
		Assert.True(high < low);
	}

	[Fact]
	public void HasLivingNeighbor_AdjacentLivingBuilding_ReturnsTrue()
	{
		IBuildingRepository repo = CreateRepo();
		List<IBuildingState> buildings = new()
		{
			CreateState("FleshNest", 0, 0, false),
		};

		bool has = LivingBuildingRules.HasLivingNeighbor(2, 0, "FleshNest", buildings, repo);

		Assert.True(has);
	}

	[Fact]
	public void HasLivingNeighbor_NonLivingOrDestroyed_ReturnsFalse()
	{
		IBuildingRepository repo = CreateRepo();
		List<IBuildingState> buildings = new()
		{
			CreateState("DemonGate", 0, 0, false),
		};

		Assert.False(LivingBuildingRules.HasLivingNeighbor(2, 0, "FleshNest", buildings, repo));

		List<IBuildingState> dead = new()
		{
			CreateState("FleshNest", 0, 0, true),
		};
		Assert.False(LivingBuildingRules.HasLivingNeighbor(2, 0, "FleshNest", dead, repo));
	}

	[Fact]
	public void HasLivingNeighbor_FarAway_ReturnsFalse()
	{
		IBuildingRepository repo = CreateRepo();
		List<IBuildingState> buildings = new()
		{
			CreateState("FleshNest", 6, 6, false),
		};

		bool has = LivingBuildingRules.HasLivingNeighbor(0, 0, "FleshNest", buildings, repo);

		Assert.False(has);
	}

	private static IBuildingRepository CreateRepo()
	{
		Mock<IBuildingRepository> repo = new();
		repo.Setup(r => r.GetFootprint(It.IsAny<string>())).Returns(new List<GridCellOffset>
		{
			new(0, 0),
			new(1, 0),
			new(0, 1),
			new(1, 1),
		});
		return repo.Object;
	}

	private static IBuildingState CreateState(string typeId, int anchorX, int anchorY, bool destroyed)
	{
		Mock<IBuildingState> state = new();
		state.SetupGet(s => s.TypeId).Returns(typeId);
		state.SetupGet(s => s.AnchorGridX).Returns(anchorX);
		state.SetupGet(s => s.AnchorGridY).Returns(anchorY);
		state.SetupGet(s => s.IsDestroyed).Returns(destroyed);
		return state.Object;
	}
}