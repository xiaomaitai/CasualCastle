using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class SlaaneshServiceRulesTests
{
	[Fact]
	public void CalculateComfortBuffs_HotSpringAdjacentToThrone_BoostsRestore()
	{
		List<IBuildingState> buildings = new()
		{
			CreateState("DungeonThroneL1", 4, 4),
			CreateState(SlaaneshServiceRules.HotSpring, 4, 2),
		};

		SlaaneshComfortBuffs buffs = SlaaneshServiceRules.CalculateComfortBuffs(buildings, CreateRepo());

		Assert.Equal(SlaaneshServiceRules.HotSpringRestoreMultiplier, buffs.RestoreMultiplier);
	}

	[Fact]
	public void CalculateComfortBuffs_NonAdjacentBuilding_NoEffect()
	{
		List<IBuildingState> buildings = new()
		{
			CreateState("DungeonThroneL1", 4, 4),
			CreateState(SlaaneshServiceRules.BanquetHall, 7, 7),
		};

		SlaaneshComfortBuffs buffs = SlaaneshServiceRules.CalculateComfortBuffs(buildings, CreateRepo());

		Assert.Equal(0f, buffs.MaxComfortBonus);
	}

	[Fact]
	public void CalculateComfortBuffs_StacksAdjacentServiceBuildings()
	{
		List<IBuildingState> buildings = new()
		{
			CreateState("DungeonThroneL1", 4, 4),
			CreateState(SlaaneshServiceRules.HotSpring, 4, 2),
			CreateState(SlaaneshServiceRules.BanquetHall, 4, 6),
			CreateState(SlaaneshServiceRules.HeartPortal, 2, 4),
			CreateState(SlaaneshServiceRules.MaidResidence, 6, 4),
		};

		SlaaneshComfortBuffs buffs = SlaaneshServiceRules.CalculateComfortBuffs(buildings, CreateRepo());

		Assert.Equal(SlaaneshServiceRules.HotSpringRestoreMultiplier, buffs.RestoreMultiplier);
		Assert.Equal(SlaaneshServiceRules.BanquetHallMaxComfortBonus, buffs.MaxComfortBonus);
		Assert.Equal(SlaaneshServiceRules.HeartPortalAttackCostMultiplier, buffs.AttackCostMultiplier);
		Assert.Equal(SlaaneshServiceRules.MaidResidenceHitCostMultiplier, buffs.HitCostMultiplier);
	}

	[Fact]
	public void CalculateComfortBuffs_PleasureStageAdjacent_SetsHalfCost()
	{
		List<IBuildingState> buildings = new()
		{
			CreateState("DungeonThroneL1", 4, 4),
			CreateState(SlaaneshServiceRules.PleasureStage, 4, 2),
		};

		SlaaneshComfortBuffs buffs = SlaaneshServiceRules.CalculateComfortBuffs(buildings, CreateRepo());

		Assert.True(buffs.HalfCostBelowHalfComfort);
	}

	[Fact]
	public void CalculateComfortBuffs_NoThrone_ReturnsDefaults()
	{
		List<IBuildingState> buildings = new()
		{
			CreateState(SlaaneshServiceRules.HotSpring, 4, 4),
		};

		SlaaneshComfortBuffs buffs = SlaaneshServiceRules.CalculateComfortBuffs(buildings, CreateRepo());

		Assert.Equal(1f, buffs.RestoreMultiplier);
		Assert.Equal(0f, buffs.MaxComfortBonus);
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

	private static IBuildingState CreateState(string typeId, int anchorX, int anchorY)
	{
		Mock<IBuildingState> state = new();
		state.SetupGet(s => s.TypeId).Returns(typeId);
		state.SetupGet(s => s.AnchorGridX).Returns(anchorX);
		state.SetupGet(s => s.AnchorGridY).Returns(anchorY);
		state.SetupGet(s => s.IsDestroyed).Returns(false);
		state.SetupGet(s => s.ContributesToAdjacency).Returns(true);
		return state.Object;
	}
}