using System;
using System.Collections.Generic;
using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class DemonKingSkillPoolTests
{
	[Fact]
	public void GenerateChoices_LevelTwo_ReturnsThreeNormalSkills()
	{
		Random random = new(42);

		List<string> choices = DemonKingSkillPool.GenerateChoices(random, 2, new List<string>());

		Assert.Equal(3, choices.Count);
		foreach (string skillId in choices)
			Assert.False(DemonKingSkillPool.IsUltimate(skillId));
	}

	[Fact]
	public void GenerateChoices_LevelFive_CanIncludeUltimate()
	{
		Random random = new(7);

		List<string> choices = DemonKingSkillPool.GenerateChoices(random, 5, new List<string>());

		Assert.Equal(3, choices.Count);
	}

	[Fact]
	public void GenerateChoices_LevelOne_ReturnsEmpty()
	{
		Random random = new(1);

		List<string> choices = DemonKingSkillPool.GenerateChoices(random, 1, new List<string>());

		Assert.Empty(choices);
	}

	[Fact]
	public void GenerateChoices_ExcludesLearnedSkills()
	{
		Random random = new(42);
		List<string> learned = new(DemonKingSkillPool.KhorneSkills[0..3]);

		List<string> choices = DemonKingSkillPool.GenerateChoices(random, 2, learned);

		foreach (string skillId in choices)
			Assert.DoesNotContain(skillId, learned);
	}

	[Fact]
	public void GetGod_CategorizesSkills()
	{
		Assert.Equal(DemonKingSkillPool.Khorne, DemonKingSkillPool.GetGod("flame_slash"));
		Assert.Equal(DemonKingSkillPool.Tzeentch, DemonKingSkillPool.GetGod("mana_storm"));
		Assert.Equal(DemonKingSkillPool.Slaanesh, DemonKingSkillPool.GetGod("feast"));
		Assert.Equal(DemonKingSkillPool.Nurgle, DemonKingSkillPool.GetGod("plague_spread"));
		Assert.Equal("", DemonKingSkillPool.GetGod("unknown"));
	}

	[Fact]
	public void IsUltimate_MatchesUltimateIds()
	{
		Assert.True(DemonKingSkillPool.IsUltimate("khorne_avatar"));
		Assert.True(DemonKingSkillPool.IsUltimate("labyrinth"));
		Assert.True(DemonKingSkillPool.IsUltimate("paradise_realm"));
		Assert.True(DemonKingSkillPool.IsUltimate("nurgle_garden"));
		Assert.False(DemonKingSkillPool.IsUltimate("blood_rage"));
	}

	[Fact]
	public void GetNormalSkillCount_MatchesDesignPool()
	{
		Assert.Equal(22, DemonKingSkillPool.GetNormalSkillCount());
	}

	[Fact]
	public void SkillArrays_EndWithUltimatePerGod()
	{
		Assert.Equal("khorne_avatar", DemonKingSkillPool.KhorneSkills[^1]);
		Assert.Equal("labyrinth", DemonKingSkillPool.TzeentchSkills[^1]);
		Assert.Equal("paradise_realm", DemonKingSkillPool.SlaaneshSkills[^1]);
		Assert.Equal("nurgle_garden", DemonKingSkillPool.NurgleSkills[^1]);
	}

	[Fact]
	public void IsKnownSkill_RejectsUnknown()
	{
		Assert.True(DemonKingSkillPool.IsKnownSkill("flame_slash"));
		Assert.False(DemonKingSkillPool.IsKnownSkill("fireball"));
	}
}