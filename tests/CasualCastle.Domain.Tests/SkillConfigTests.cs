using System.Collections.Generic;
using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class SkillConfigTests
{
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
}
