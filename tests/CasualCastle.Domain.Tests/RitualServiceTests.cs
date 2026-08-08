using CasualCastle.Domain.Building;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class RitualServiceTests
{
	[Fact]
	public void TryStartRitual_CoreLevelBelowRequirement_ReturnsNullWithoutSpend()
	{
		RitualService service = new();
		bool goldSpent = false;
		bool magicSpent = false;

		ActiveRitual ritual = service.TryStartRitual(
			RitualRules.BoneReinforceId, false, 1,
			_ => goldSpent = true,
			_ => magicSpent = true);

		Assert.Null(ritual);
		Assert.False(goldSpent);
		Assert.False(magicSpent);
	}

	[Fact]
	public void TryStartRitual_InsufficientBlackMagic_ReturnsNull()
	{
		RitualService service = new();

		ActiveRitual ritual = service.TryStartRitual(
			RitualRules.BlackMagicChargeId, false, 1,
			_ => true,
			_ => false);

		Assert.Null(ritual);
	}

	[Fact]
	public void TryStartRitual_BlackMagicCost_SpendsAndStarts()
	{
		RitualService service = new();
		int spentAmount = 0;

		ActiveRitual ritual = service.TryStartRitual(
			RitualRules.BlackMagicChargeId, false, 1,
			_ => true,
			amount => { spentAmount = amount; return true; });

		Assert.NotNull(ritual);
		Assert.Equal(20, spentAmount);
		Assert.Equal(10f, ritual.RemainingSeconds);
		Assert.Equal(10f, ritual.TotalSeconds);
		Assert.False(ritual.IsCoreHosted);
	}

	[Fact]
	public void TryStartRitual_GoldCost_SpendsAndStarts()
	{
		RitualService service = new();
		int spentAmount = 0;

		ActiveRitual ritual = service.TryStartRitual(
			RitualRules.UndeadSummonId, true, 1,
			amount => { spentAmount = amount; return true; },
			_ => true);

		Assert.NotNull(ritual);
		Assert.Equal(30, spentAmount);
		Assert.True(ritual.IsCoreHosted);
	}

	[Fact]
	public void TryStartRitual_UnknownRitual_ReturnsNull()
	{
		RitualService service = new();

		ActiveRitual ritual = service.TryStartRitual(
			"unknown", false, 5,
			_ => true,
			_ => true);

		Assert.Null(ritual);
	}

	[Fact]
	public void Tick_DecrementsRemainingUntilComplete()
	{
		RitualService service = new();
		ActiveRitual ritual = service.TryStartRitual(
			RitualRules.UndeadSummonId, false, 1,
			_ => true,
			_ => true);

		service.Tick(ritual, 6f);

		Assert.False(ritual.IsComplete);
		Assert.Equal(4f, ritual.RemainingSeconds, 4);

		service.Tick(ritual, 4f);

		Assert.True(ritual.IsComplete);
	}

	[Fact]
	public void GetReward_NonCoreHosted_ReturnsBasicReward()
	{
		RitualService service = new();
		ActiveRitual ritual = service.TryStartRitual(
			RitualRules.UndeadSummonId, false, 1,
			_ => true,
			_ => true);

		RitualReward reward = service.GetReward(ritual);

		Assert.Equal("Skeleton", reward.SummonUnitTypeId);
		Assert.Equal(3, reward.SummonCount);
	}

	[Fact]
	public void GetReward_CoreHosted_ReturnsCoreReward()
	{
		RitualService service = new();
		ActiveRitual ritual = service.TryStartRitual(
			RitualRules.UndeadSummonId, true, 1,
			_ => true,
			_ => true);

		RitualReward reward = service.GetReward(ritual);

		Assert.Equal(5, reward.SummonCount);
	}

	[Fact]
	public void GetReward_CoreHostedBoneReinforce_ReturnsHealthBonus()
	{
		RitualService service = new();
		ActiveRitual ritual = service.TryStartRitual(
			RitualRules.BoneReinforceId, true, 2,
			_ => true,
			_ => true);

		RitualReward reward = service.GetReward(ritual);

		Assert.Equal(10, reward.UndeadMaxHealthBonus);
		Assert.Null(reward.SummonUnitTypeId);
	}

	[Fact]
	public void GetReward_BlackMagicCharge_ReturnsGold()
	{
		RitualService service = new();
		ActiveRitual ritual = service.TryStartRitual(
			RitualRules.BlackMagicChargeId, true, 1,
			_ => true,
			_ => true);

		RitualReward reward = service.GetReward(ritual);

		Assert.Equal(30, reward.GoldAmount);
	}

	[Fact]
	public void CanHostRitual_AltarAndCircle_ReturnsTrue()
	{
		Assert.True(RitualRules.CanHostRitual("DarkAltar"));
		Assert.True(RitualRules.CanHostRitual("RitualCircleL1"));
		Assert.True(RitualRules.CanHostRitual("RitualCircleL5"));
	}

	[Fact]
	public void CanHostRitual_OtherBuilding_ReturnsFalse()
	{
		Assert.False(RitualRules.CanHostRitual("Tomb"));
		Assert.False(RitualRules.CanHostRitual(null));
	}

	[Fact]
	public void IsRitualCircleHost_CirclePrefix_ReturnsTrue()
	{
		Assert.True(RitualRules.IsRitualCircleHost("RitualCircleL3"));
		Assert.False(RitualRules.IsRitualCircleHost("DarkAltar"));
	}

	[Fact]
	public void GetAll_ReturnsThreeRituals()
	{
		Assert.Equal(3, RitualRules.GetAll().Count);
	}

	[Fact]
	public void Get_UnknownId_ReturnsNull()
	{
		Assert.Null(RitualRules.Get("unknown"));
	}
}