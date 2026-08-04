using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class FurySkillTests
{
	private static SkillSet CreateFurySkill(string json)
	{
		SkillSet skills = new();
		skills.Add(new SkillDef
		{
			Id = "fury",
			DisplayName = "Fury",
			Type = SkillType.StatModifier,
			ConfigJson = json
		});
		return skills;
	}

	[Theory]
	[InlineData(1.00f, 1.0f)]
	[InlineData(0.85f, 1.0f)]
	[InlineData(0.80f, 1.1f)]
	[InlineData(0.70f, 1.1f)]
	[InlineData(0.60f, 1.2f)]
	[InlineData(0.50f, 1.2f)]
	[InlineData(0.40f, 1.3f)]
	[InlineData(0.30f, 1.3f)]
	[InlineData(0.20f, 1.4f)]
	[InlineData(0.10f, 1.4f)]
	public void FuryT1_AttackMultiplier_ScalesByHealthLost(float hpRatio, float expected)
	{
		SkillSet skills = CreateFurySkill("{\"trigger\":\"low_health_step\",\"tier\":1}");
		GameContext ctx = new() { CurrentHpRatio = hpRatio };

		float mult = skills.GetStatMultiplier("attack_damage_mult", ctx);

		Assert.Equal(expected, mult, 4);
	}

	[Theory]
	[InlineData(1.00f, 1.0f)]
	[InlineData(0.85f, 1.12f)]
	[InlineData(0.70f, 1.24f)]
	[InlineData(0.55f, 1.36f)]
	[InlineData(0.40f, 1.48f)]
	[InlineData(0.25f, 1.48f)]
	[InlineData(0.05f, 1.48f)]
	public void FuryT2_AttackMultiplier_ScalesAndCaps(float hpRatio, float expected)
	{
		SkillSet skills = CreateFurySkill("{\"trigger\":\"low_health_step\",\"tier\":2}");
		GameContext ctx = new() { CurrentHpRatio = hpRatio };

		float mult = skills.GetStatMultiplier("attack_damage_mult", ctx);

		Assert.Equal(expected, mult, 4);
	}

	[Theory]
	[InlineData(1.00f, 1.0f)]
	[InlineData(0.85f, 1.08f)]
	[InlineData(0.70f, 1.16f)]
	[InlineData(0.55f, 1.24f)]
	[InlineData(0.40f, 1.32f)]
	[InlineData(0.25f, 1.32f)]
	public void FuryT2_AttackSpeedMultiplier_ScalesAndCaps(float hpRatio, float expected)
	{
		SkillSet skills = CreateFurySkill("{\"trigger\":\"low_health_step\",\"tier\":2}");
		GameContext ctx = new() { CurrentHpRatio = hpRatio };

		float mult = skills.GetStatMultiplier("attack_speed_mult", ctx);

		Assert.Equal(expected, mult, 4);
	}

	[Theory]
	[InlineData(0.60f, 1.0f)]
	[InlineData(0.50f, 2.0f)]
	[InlineData(0.40f, 2.0f)]
	public void LowHealthThreshold_ExistingSemanticsUnchanged(float hpRatio, float expected)
	{
		SkillSet skills = CreateFurySkill("{\"trigger\":\"low_health\",\"trigger_param\":0.5,\"modifiers\":{\"attack_speed_mult\":2.0}}");
		GameContext ctx = new() { CurrentHpRatio = hpRatio };

		float mult = skills.GetStatMultiplier("attack_speed_mult", ctx);

		Assert.Equal(expected, mult, 4);
	}
}
