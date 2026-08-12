using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class DemonKingSkillScalingTests
{
	[Fact]
	public void Scale_StatModifier_ScalesMultiplierByLevelRatio()
	{
		SkillDef axe = new()
		{
			Id = "khorne_axe",
			DisplayName = "恐虐之斧",
			Type = SkillType.StatModifier,
			ConfigJson = "{\"trigger\":\"always\",\"race\":\"DemonKing\",\"modifiers\":{\"attack_damage_mult\":1.15}}"
		};

		SkillDef scaled = DemonKingSkillScaling.Scale(axe, 2, 4);

		StatModifierConfig config = StatModifierConfig.Parse(scaled.ConfigJson);
		Assert.Equal(1.3f, config.GetMultiplier("attack_damage_mult"), 3);
		Assert.Equal("DemonKing", config.RaceRequirement);
		Assert.Equal(StatModifierTrigger.Always, config.Trigger);
	}

	[Fact]
	public void Scale_AtSameLevel_ReturnsUnchanged()
	{
		SkillDef axe = new()
		{
			Id = "khorne_axe",
			DisplayName = "恐虐之斧",
			Type = SkillType.StatModifier,
			ConfigJson = "{\"trigger\":\"always\",\"race\":\"DemonKing\",\"modifiers\":{\"attack_damage_mult\":1.15}}"
		};

		SkillDef scaled = DemonKingSkillScaling.Scale(axe, 2, 2);

		Assert.Equal(1.15f, StatModifierConfig.Parse(scaled.ConfigJson).GetMultiplier("attack_damage_mult"), 3);
	}

	[Fact]
	public void Scale_NonStatModifier_ReturnsUnchanged()
	{
		SkillDef aura = new()
		{
			Id = "war_horn",
			DisplayName = "战争号角",
			Type = SkillType.Aura,
			ConfigJson = "{\"target\":\"nearby_allies\",\"range\":180,\"race\":\"Demon\",\"bonus_type\":\"attack_damage\",\"value\":0.15}"
		};

		SkillDef scaled = DemonKingSkillScaling.Scale(aura, 2, 5);

		Assert.Equal(aura.ConfigJson, scaled.ConfigJson);
	}
}