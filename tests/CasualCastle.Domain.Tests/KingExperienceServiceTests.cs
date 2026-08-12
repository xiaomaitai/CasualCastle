using System.Collections.Generic;
using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class KingExperienceServiceTests
{
	[Fact]
	public void InitialState_IsLevelOne()
	{
		KingExperienceService xp = new();

		Assert.Equal(1, xp.Level);
		Assert.Equal(0, xp.Experience);
	}

	[Fact]
	public void OnUnitDied_FriendlyGrantsLowXp()
	{
		KingExperienceService xp = new();

		xp.OnUnitDied(true);

		Assert.Equal(DemonKingRules.FriendlyDeathXp, xp.Experience);
	}

	[Fact]
	public void OnUnitDied_EnemyGrantsMoreXp()
	{
		KingExperienceService xp = new();

		xp.OnUnitDied(false);

		Assert.Equal(DemonKingRules.EnemyDeathXp, xp.Experience);
	}

	[Fact]
	public void OnKingKill_AddsKillBonus()
	{
		KingExperienceService xp = new();

		xp.OnUnitDied(false);
		xp.OnKingKill();

		Assert.Equal(DemonKingRules.EnemyDeathXp + DemonKingRules.KingKillXp, xp.Experience);
	}

	[Fact]
	public void OnCrystalUsed_GrantsHighXp()
	{
		KingExperienceService xp = new();

		xp.OnCrystalUsed();

		Assert.Equal(DemonKingRules.CrystalXp, xp.Experience);
	}

	[Fact]
	public void AddExperience_LevelsUpAndCarriesOverflow()
	{
		KingExperienceService xp = new();
		List<int> levelUps = new();

		xp.LevelUp += level => levelUps.Add(level);
		xp.AddExperience(DemonKingRules.GetRequiredExperienceForLevel(1) + 30);

		Assert.Equal(2, xp.Level);
		Assert.Equal(30, xp.Experience);
		Assert.Equal(new List<int> { 2 }, levelUps);
	}

	[Fact]
	public void AddExperience_MultiLevelUp_FiresEachLevel()
	{
		KingExperienceService xp = new();
		int total = DemonKingRules.GetRequiredExperienceForLevel(1)
			+ DemonKingRules.GetRequiredExperienceForLevel(2)
			+ 10;
		List<int> levelUps = new();

		xp.LevelUp += level => levelUps.Add(level);
		xp.AddExperience(total);

		Assert.Equal(3, xp.Level);
		Assert.Equal(10, xp.Experience);
		Assert.Equal(new List<int> { 2, 3 }, levelUps);
	}

	[Fact]
	public void AddExperience_CappedAtLevelFive()
	{
		KingExperienceService xp = new();
		xp.AddExperience(100000);

		Assert.Equal(5, xp.Level);
		Assert.Equal(0, xp.GetRemainingExperienceToNextLevel());
	}

	[Fact]
	public void GetRemainingExperienceToNextLevel_ReturnsDifference()
	{
		KingExperienceService xp = new();
		xp.AddExperience(30);

		Assert.Equal(DemonKingRules.GetRequiredExperienceForLevel(1) - 30, xp.GetRemainingExperienceToNextLevel());
	}

	[Fact]
	public void Restore_SetsLevelFromCumulativeExperience()
	{
		KingExperienceService xp = new();
		int total = DemonKingRules.GetRequiredExperienceForLevel(1)
			+ DemonKingRules.GetRequiredExperienceForLevel(2)
			+ 15;

		xp.Restore(total);

		Assert.Equal(3, xp.Level);
		Assert.Equal(15, xp.Experience);
		Assert.Equal(total, xp.GetCumulativeExperience());
	}

	[Fact]
	public void Reset_RestoresLevelOne()
	{
		KingExperienceService xp = new();
		xp.AddExperience(500);

		xp.Reset();

		Assert.Equal(1, xp.Level);
		Assert.Equal(0, xp.Experience);
	}
}