using System.Collections.Generic;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Shared;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class SkillConfigTests
{
	[Fact]
	public void SkillDef_Description_StoredOnSkill()
	{
		SkillDef skill = new()
		{
			Id = "orc_rage_t1",
			DisplayName = "狂怒",
			Description = "每损失 20% 生命，攻击力 +10%（最高 +40%）",
			Type = SkillType.StatModifier,
			ConfigJson = "{}"
		};

		Assert.Equal("每损失 20% 生命，攻击力 +10%（最高 +40%）", skill.Description);
	}

	[Fact]
	public void StatModifierConfig_Parse_TriggerAlways_ReturnsCorrectConfig()
	{
		StatModifierConfig cfg = StatModifierConfig.Parse("{\"trigger\": \"always\", \"modifiers\": {\"attack_damage_mult\": 1.5}}");
		Assert.Equal(StatModifierTrigger.Always, cfg.Trigger);
		Assert.Equal(1.5f, cfg.GetMultiplier("attack_damage_mult"));
	}

	[Fact]
	public void StatModifierConfig_Parse_LowHealth_ReturnsCorrectConfig()
	{
		StatModifierConfig cfg = StatModifierConfig.Parse("{\"trigger\": \"low_health\", \"trigger_param\": 0.3, \"modifiers\": {\"attack_speed_mult\": 2.0}}");
		Assert.Equal(StatModifierTrigger.LowHealth, cfg.Trigger);
		Assert.Equal(0.3f, cfg.TriggerParam);
		Assert.Equal(2.0f, cfg.GetMultiplier("attack_speed_mult"));
	}

	[Fact]
	public void StatModifierConfig_GetMultiplier_UnknownKey_ReturnsDefault()
	{
		StatModifierConfig cfg = StatModifierConfig.Parse("{\"trigger\": \"always\", \"modifiers\": {}}");
		Assert.Equal(1f, cfg.GetMultiplier("unknown_key"));
	}

	[Fact]
	public void StatModifierConfig_GetMultiplier_DodgeChance_ReturnsZero()
	{
		StatModifierConfig cfg = StatModifierConfig.Parse("{\"trigger\": \"always\", \"modifiers\": {}}");
		Assert.Equal(0f, cfg.GetMultiplier("dodge_chance"));
	}

	[Fact]
	public void OnHitConfig_Parse_ReturnsCorrectEffects()
	{
		OnHitConfig cfg = OnHitConfig.Parse("{\"effects\": [{\"type\": \"slow\", \"value\": 0.3, \"duration\": 2.0}]}");
		Assert.Single(cfg.Effects);
		Assert.Equal("slow", cfg.Effects[0].Type);
		Assert.Equal(0.3f, cfg.Effects[0].Value);
		Assert.Equal(2.0f, cfg.Effects[0].Duration);
	}

	[Fact]
	public void StatModifierConfig_Parse_OnDamaged_ReturnsCorrectConfig()
	{
		StatModifierConfig cfg = StatModifierConfig.Parse("{\"trigger\":\"on_damaged\",\"race\":\"Orc\",\"modifiers\":{\"attack_damage_mult\":1.3,\"move_speed_mult\":1.15}}");
		Assert.Equal(StatModifierTrigger.OnDamaged, cfg.Trigger);
		Assert.Equal("Orc", cfg.RaceRequirement);
		Assert.Equal(1.3f, cfg.GetMultiplier("attack_damage_mult"));
		Assert.Equal(1.15f, cfg.GetMultiplier("move_speed_mult"));
	}

	[Fact]
	public void StatModifierConfig_Parse_NoRace_UsesNullRequirement()
	{
		StatModifierConfig cfg = StatModifierConfig.Parse("{\"trigger\":\"on_damaged\",\"modifiers\":{\"attack_damage_mult\":1.3}}");
		Assert.Null(cfg.RaceRequirement);
	}

	[Fact]
	public void AuraConfig_Parse_NearbyAllies_ReturnsCorrectConfig()
	{
		AuraConfig cfg = AuraConfig.Parse("{\"target\": \"nearby_enemies\", \"range\": 200}");
		Assert.Equal(AuraTarget.NearbyEnemies, cfg.Target);
		Assert.Equal(200f, cfg.Range);
	}

	[Fact]
	public void AuraConfig_Parse_DefaultTarget_ReturnsNearbyAllies()
	{
		AuraConfig cfg = AuraConfig.Parse("{\"target\": \"unknown\", \"range\": 150}");
		Assert.Equal(AuraTarget.NearbyAllies, cfg.Target);
	}

	[Fact]
	public void AuraConfig_Parse_MultiEffect_ReturnsBothEffects()
	{
		AuraConfig cfg = AuraConfig.Parse("{\"target\":\"nearby_allies\",\"range\":150,\"race\":\"Undead\",\"effects\":[{\"bonus_type\":\"attack_damage\",\"value\":0.20},{\"bonus_type\":\"move_speed\",\"value\":0.15}]}");

		Assert.Equal(2, cfg.Effects.Count);
		Assert.Equal("attack_damage", cfg.Effects[0].BonusType);
		Assert.Equal(0.20f, cfg.Effects[0].Value, 4);
		Assert.Equal("move_speed", cfg.Effects[1].BonusType);
		Assert.Equal(0.15f, cfg.Effects[1].Value, 4);
	}

	[Fact]
	public void AuraConfig_Parse_SingleEffect_BackwardCompatible()
	{
		AuraConfig cfg = AuraConfig.Parse("{\"target\":\"nearby_allies\",\"range\":150,\"race\":\"Goblin\",\"bonus_type\":\"attack_speed\",\"value\":0.12}");

		Assert.Single(cfg.Effects);
		Assert.Equal("attack_speed", cfg.Effects[0].BonusType);
		Assert.Equal(0.12f, cfg.Effects[0].Value, 4);
	}

	[Fact]
	public void SpecialConfig_Parse_BehaviorKiting_ReturnsCorrectConfig()
	{
		SpecialConfig cfg = SpecialConfig.Parse("{\"behavior\": \"kiting\", \"param\": 100}");
		Assert.Equal(SpecialBehavior.Kiting, cfg.Behavior);
		Assert.Equal(100f, cfg.Param);
	}

	[Fact]
	public void SpecialConfig_Parse_NoParam_DefaultsToZero()
	{
		SpecialConfig cfg = SpecialConfig.Parse("{\"behavior\": \"stealth\"}");
		Assert.Equal(SpecialBehavior.Stealth, cfg.Behavior);
		Assert.Equal(0f, cfg.Param);
	}

	[Fact]
	public void SpecialConfig_Parse_WithSpeedMult_ReturnsCorrectConfig()
	{
		SpecialConfig cfg = SpecialConfig.Parse("{\"behavior\": \"stealth\", \"param\": 180, \"speed_mult\": 0.85}");
		Assert.Equal(SpecialBehavior.Stealth, cfg.Behavior);
		Assert.Equal(180f, cfg.Param);
		Assert.Equal(0.85f, cfg.SpeedMult);
	}

	[Fact]
	public void SpecialConfig_Parse_NoSpeedMult_DefaultsToGameRule()
	{
		SpecialConfig cfg = SpecialConfig.Parse("{\"behavior\": \"stealth\", \"param\": 180}");
		Assert.Equal(GameRules.StealthSpeedMult, cfg.SpeedMult);
	}

	[Fact]
	public void SpecialConfig_Parse_WithCooldown_ReturnsCorrectConfig()
	{
		SpecialConfig cfg = SpecialConfig.Parse("{\"behavior\": \"stealth\", \"cooldown\": 3.0}");
		Assert.Equal(3f, cfg.Cooldown);
	}

	[Fact]
	public void SpecialConfig_Parse_NoCooldown_DefaultsToZero()
	{
		SpecialConfig cfg = SpecialConfig.Parse("{\"behavior\": \"stealth\", \"param\": 180}");
		Assert.Equal(0f, cfg.Cooldown);
	}

	[Fact]
	public void SpecialConfig_Parse_Curse_ReturnsCostAndValue()
	{
		SpecialConfig cfg = SpecialConfig.Parse("{\"behavior\":\"curse\",\"param\":4.0,\"cost\":20,\"value\":0.25}");

		Assert.Equal(SpecialBehavior.Curse, cfg.Behavior);
		Assert.Equal(4f, cfg.Param);
		Assert.Equal(20, cfg.Cost);
		Assert.Equal(0.25f, cfg.Value, 4);
	}

	[Fact]
	public void SpecialConfig_Parse_ShadowNova_ReturnsCostAndValue()
	{
		SpecialConfig cfg = SpecialConfig.Parse("{\"behavior\":\"shadow_nova\",\"param\":120,\"cost\":40,\"value\":30,\"cooldown\":6.0}");

		Assert.Equal(SpecialBehavior.ShadowNova, cfg.Behavior);
		Assert.Equal(120f, cfg.Param);
		Assert.Equal(40, cfg.Cost);
		Assert.Equal(30f, cfg.Value, 4);
		Assert.Equal(6f, cfg.Cooldown);
	}
}
