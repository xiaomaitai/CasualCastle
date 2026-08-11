using System.Collections.Generic;
using CasualCastle.Domain.Building;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class RitualServiceTests
{
	[Fact]
	public void TryStartRitual_CoreLevelBelowRequirement_ReturnsNullWithoutSpend()
	{
		RitualService service = new();
		service.Learn(RitualRules.BoneReinforceId);
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
		service.Learn(RitualRules.BlackMagicChargeId);

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
		service.Learn(RitualRules.BlackMagicChargeId);
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
		service.Learn(RitualRules.UndeadSummonId);
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
		service.Learn(RitualRules.UndeadSummonId);
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
		service.Learn(RitualRules.UndeadSummonId);
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
		service.Learn(RitualRules.UndeadSummonId);
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
		service.Learn(RitualRules.BoneReinforceId);
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
		service.Learn(RitualRules.BlackMagicChargeId);
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
	public void GetAll_ReturnsElevenRituals()
	{
		Assert.Equal(11, RitualRules.GetAll().Count);
	}

	[Fact]
	public void Get_UnknownId_ReturnsNull()
	{
		Assert.Null(RitualRules.Get("unknown"));
	}

	[Fact]
	public void TryStartRitual_UnlearnedRitual_ReturnsNull()
	{
		RitualService service = new();

		ActiveRitual ritual = service.TryStartRitual(
			RitualRules.BlackMagicChargeId, false, 1,
			_ => true,
			_ => true);

		Assert.Null(ritual);
	}

	[Fact]
	public void TryStartRitual_StudyWithoutBook_ReturnsNullWithoutSpend()
	{
		RitualService service = new();
		bool magicSpent = false;

		ActiveRitual ritual = service.TryStartRitual(
			RitualRules.DarkMagicStudyId, false, 1,
			_ => true,
			_ => magicSpent = true,
			null, null);

		Assert.Null(ritual);
		Assert.False(magicSpent);
	}

	[Fact]
	public void TryStartRitual_StudyWithAlreadyLearnedBook_ReturnsNull()
	{
		RitualService service = new();
		service.Learn(RitualRules.UndeadSummonId);

		ActiveRitual ritual = service.TryStartRitual(
			RitualRules.DarkMagicStudyId, true, 1,
			_ => true,
			_ => true,
			"book_1", RitualRules.UndeadSummonId);

		Assert.Null(ritual);
	}

	[Fact]
	public void TryStartRitual_StudyFirstCompletion_SpendsBaseCost()
	{
		RitualService service = new();
		int spentAmount = 0;

		ActiveRitual ritual = service.TryStartRitual(
			RitualRules.DarkMagicStudyId, false, 1,
			_ => true,
			amount => { spentAmount = amount; return true; },
			"book_1", RitualRules.BlackMagicChargeId);

		Assert.NotNull(ritual);
		Assert.Equal(DarkMagicStudyRules.BaseCost, spentAmount);
		Assert.Equal("book_1", ritual.BookCardId);
		Assert.Equal(RitualRules.BlackMagicChargeId, ritual.BookRitualId);
	}

	[Fact]
	public void TryStartRitual_StudyAfterOneCompletion_SpendsEscalatedCost()
	{
		RitualService service = new();
		ActiveRitual first = service.TryStartRitual(
			RitualRules.DarkMagicStudyId, false, 1,
			_ => true,
			_ => true,
			"book_1", RitualRules.BlackMagicChargeId);
		service.Tick(first, 10f);
		service.Complete(first);

		int spentAmount = 0;
		ActiveRitual second = service.TryStartRitual(
			RitualRules.DarkMagicStudyId, false, 1,
			_ => true,
			amount => { spentAmount = amount; return true; },
			"book_2", RitualRules.UndeadSummonId);

		Assert.NotNull(second);
		Assert.Equal(DarkMagicStudyRules.BaseCost + DarkMagicStudyRules.CostIncrementPerCompletion, spentAmount);
	}

	[Fact]
	public void TryStartRitual_StudyBelowCoreLevelRequirement_ReturnsNull()
	{
		RitualService service = new();

		ActiveRitual ritual = service.TryStartRitual(
			RitualRules.DarkMagicStudyId, true, 0,
			_ => true,
			_ => true,
			"book_1", RitualRules.UndeadSummonId);

		Assert.Null(ritual);
	}

	[Fact]
	public void Complete_StudyAtCore_LearnsBookRitualAndMarksLevelUp()
	{
		RitualService service = new();
		ActiveRitual ritual = service.TryStartRitual(
			RitualRules.DarkMagicStudyId, true, 1,
			_ => true,
			_ => true,
			"book_1", RitualRules.UndeadSummonId);

		service.Tick(ritual, 10f);
		RitualReward reward = service.Complete(ritual);

		Assert.Equal(RitualRules.UndeadSummonId, reward.LearnedRitualId);
		Assert.True(reward.CoreLevelUp);
		Assert.True(service.IsLearned(RitualRules.UndeadSummonId));
		Assert.Equal(1, service.StudyCompletionCount);
	}

	[Fact]
	public void Complete_StudyAtAltar_LearnsBookRitualWithoutLevelUp()
	{
		RitualService service = new();
		ActiveRitual ritual = service.TryStartRitual(
			RitualRules.DarkMagicStudyId, false, 1,
			_ => true,
			_ => true,
			"book_1", RitualRules.BlackMagicChargeId);

		RitualReward reward = service.Complete(ritual);

		Assert.Equal(RitualRules.BlackMagicChargeId, reward.LearnedRitualId);
		Assert.False(reward.CoreLevelUp);
		Assert.True(service.IsLearned(RitualRules.BlackMagicChargeId));
	}

	[Fact]
	public void GetReward_Study_ReturnsLearnedRitualAndCoreLevelUp()
	{
		RitualService service = new();
		ActiveRitual ritual = service.TryStartRitual(
			RitualRules.DarkMagicStudyId, true, 2,
			_ => true,
			_ => true,
			"book_1", RitualRules.BoneReinforceId);

		RitualReward reward = service.GetReward(ritual);

		Assert.Equal(RitualRules.BoneReinforceId, reward.LearnedRitualId);
		Assert.True(reward.CoreLevelUp);
	}

	[Fact]
	public void GetAvailableRituals_Initially_OnlyStudyRitual()
	{
		RitualService service = new();

		IReadOnlyList<RitualDef> available = service.GetAvailableRituals();

		Assert.Single(available);
		Assert.Equal(RitualRules.DarkMagicStudyId, available[0].Id);
	}

	[Fact]
	public void GetAvailableRituals_AfterLearn_IncludesLearnedRitual()
	{
		RitualService service = new();
		service.Learn(RitualRules.UndeadSummonId);

		IReadOnlyList<RitualDef> available = service.GetAvailableRituals();

		Assert.Equal(2, available.Count);
		Assert.Contains(available, def => def.Id == RitualRules.UndeadSummonId);
	}

	[Fact]
	public void Learn_AlreadyLearned_ReturnsFalse()
	{
		RitualService service = new();
		Assert.True(service.Learn(RitualRules.UndeadSummonId));
		Assert.False(service.Learn(RitualRules.UndeadSummonId));
	}

	[Fact]
	public void Reset_ClearsLearnedRitualsAndCompletionCount()
	{
		RitualService service = new();
		service.Learn(RitualRules.UndeadSummonId);
		ActiveRitual ritual = service.TryStartRitual(
			RitualRules.DarkMagicStudyId, false, 1,
			_ => true,
			_ => true,
			"book_1", RitualRules.BlackMagicChargeId);
		service.Complete(ritual);

		service.Reset();

		Assert.False(service.IsLearned(RitualRules.UndeadSummonId));
		Assert.False(service.IsLearned(RitualRules.BlackMagicChargeId));
		Assert.Equal(0, service.StudyCompletionCount);
	}
	[Fact]
	public void Restore_RestoresCompletionCountAndLearnedRituals()
	{
		RitualService service = new();
		service.Restore(2, new List<string> { RitualRules.UndeadSummonId, RitualRules.BlackMagicChargeId });

		Assert.Equal(2, service.StudyCompletionCount);
		Assert.True(service.IsLearned(RitualRules.UndeadSummonId));
		Assert.True(service.IsLearned(RitualRules.BlackMagicChargeId));
		Assert.Equal(2, service.GetLearnedRitualIds().Count);
	}

	[Fact]
	public void IsDarkAltarUnlocked_Initially_ReturnsFalse()
	{
		RitualService service = new();

		Assert.False(service.IsDarkAltarUnlocked);
	}

	[Fact]
	public void Learn_EvilRitual_UnlocksDarkAltar()
	{
		RitualService service = new();

		Assert.True(service.Learn(RitualRules.EvilRitualId));

		Assert.True(service.IsDarkAltarUnlocked);
	}

	[Fact]
	public void TryStartRitual_StudyBookAboveCoreLevel_ReturnsNullWithoutSpend()
	{
		RitualService service = new();
		bool magicSpent = false;

		ActiveRitual ritual = service.TryStartRitual(
			RitualRules.DarkMagicStudyId, true, 1,
			_ => true,
			_ => magicSpent = true,
			"book_high", RitualRules.DeadRiteId);

		Assert.Null(ritual);
		Assert.False(magicSpent);
	}

	[Fact]
	public void TryStartRitual_StudyBookAtCoreLevel_Starts()
	{
		RitualService service = new();
		bool magicSpent = false;

		ActiveRitual ritual = service.TryStartRitual(
			RitualRules.DarkMagicStudyId, true, 2,
			_ => true,
			_ => magicSpent = true,
			"book_lv2", RitualRules.BoneReinforceId);

		Assert.NotNull(ritual);
		Assert.True(magicSpent);
	}

	[Fact]
	public void GetReward_ShadowContractCore_ReturnsBlackMagicAmount()
	{
		RitualService service = new();
		service.Learn(RitualRules.ShadowContractId);
		ActiveRitual ritual = service.TryStartRitual(
			RitualRules.ShadowContractId, true, 2,
			_ => true,
			_ => true);

		RitualReward reward = service.GetReward(ritual);

		Assert.Equal(60, reward.BlackMagicAmount);
	}

	[Fact]
	public void GetReward_DeadRiteCore_ReturnsDeathLordAndDamageMultiplier()
	{
		RitualService service = new();
		service.Learn(RitualRules.DeadRiteId);
		ActiveRitual ritual = service.TryStartRitual(
			RitualRules.DeadRiteId, true, 5,
			_ => true,
			_ => true);

		RitualReward reward = service.GetReward(ritual);

		Assert.Equal("DeathLord", reward.SummonUnitTypeId);
		Assert.Equal(1, reward.SummonCount);
		Assert.Equal(0.10f, reward.UndeadDamageMultiplier);
	}
}