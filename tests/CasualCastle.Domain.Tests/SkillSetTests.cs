using System.Collections.Generic;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Shared;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class SkillSetTests
{
	[Fact]
	public void Add_IncreasesCount()
	{
		SkillSet skills = new();
		SkillDef skill = new() { Id = "s1", DisplayName = "Test", Type = SkillType.StatModifier, ConfigJson = "{}" };
		skills.Add(skill);
		Assert.Equal(1, skills.Count);
	}

	[Fact]
	public void Remove_ExistingSkill_ReturnsTrueAndDecreasesCount()
	{
		SkillSet skills = new();
		SkillDef skill = new() { Id = "s1", DisplayName = "Test", Type = SkillType.StatModifier, ConfigJson = "{}" };
		skills.Add(skill);
		bool removed = skills.Remove("s1");
		Assert.True(removed);
		Assert.Equal(0, skills.Count);
	}

	[Fact]
	public void Remove_NonExistingSkill_ReturnsFalse()
	{
		SkillSet skills = new();
		bool removed = skills.Remove("s1");
		Assert.False(removed);
	}

	[Fact]
	public void HasSkill_ExistingSkill_ReturnsTrue()
	{
		SkillSet skills = new();
		SkillDef skill = new() { Id = "s1", DisplayName = "Test", Type = SkillType.StatModifier, ConfigJson = "{}" };
		skills.Add(skill);
		Assert.True(skills.HasSkill("s1"));
	}

	[Fact]
	public void HasSkill_NonExistingSkill_ReturnsFalse()
	{
		SkillSet skills = new();
		Assert.False(skills.HasSkill("s1"));
	}

	[Fact]
	public void GetStatMultiplier_NoSkills_ReturnsDefault()
	{
		SkillSet skills = new();
		GameContext ctx = new();
		Assert.Equal(1f, skills.GetStatMultiplier("attack_speed_mult", ctx));
	}

	[Fact]
	public void GetDodgeChance_SingleRace_ReturnsZero()
	{
		SkillSet skills = new();
		GameContext ctx = new() { NearbyAllyRaces = new HashSet<string> { "Human" } };
		Assert.Equal(0f, skills.GetDodgeChance(ctx));
	}

	[Fact]
	public void GetDodgeChance_NoDodgeSkill_ReturnsZero()
	{
		SkillSet skills = new();
		GameContext ctx = new() { NearbyAllyRaces = new HashSet<string> { "Human", "Elf", "Dwarf" } };
		Assert.Equal(0f, skills.GetDodgeChance(ctx));
	}

	[Fact]
	public void GetDodgeChance_WithDodgeSkill_AppliesBaseChanceAndMultiplier()
	{
		SkillSet skills = new();
		GameContext ctx = new() { NearbyAllyRaces = new HashSet<string> { "Human", "Elf", "Dwarf" } };
		SkillDef skill = new()
		{
			Id = "dodge_1",
			DisplayName = "Dodge",
			Type = SkillType.StatModifier,
			ConfigJson = "{\"trigger\": \"always\", \"modifiers\": {\"dodge_chance\": 1.0}}"
		};
		skills.Add(skill);
		float expected = GameRules.DodgeChanceByAllyRaceCount[2];
		Assert.Equal(expected, skills.GetDodgeChance(ctx));
	}

	[Fact]
	public void HasTrigger_WithMatchingTrigger_ReturnsTrue()
	{
		SkillSet skills = new();
		SkillDef skill = new()
		{
			Id = "s1",
			DisplayName = "Test",
			Type = SkillType.StatModifier,
			ConfigJson = "{\"trigger\": \"always\", \"modifiers\": {}}"
		};
		skills.Add(skill);
		Assert.True(skills.HasTrigger(StatModifierTrigger.Always));
	}

	[Fact]
	public void HasTrigger_NoMatchingTrigger_ReturnsFalse()
	{
		SkillSet skills = new();
		Assert.False(skills.HasTrigger(StatModifierTrigger.LowHealth));
	}
}
