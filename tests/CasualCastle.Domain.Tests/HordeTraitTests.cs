using System.Collections.Generic;
using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class GoblinSwarmTests
{
	private const string SwarmT1Config = "{\"trigger\":\"nearby_race_count\",\"race\":\"Goblin\",\"count_bonus\":0.06,\"count_cap\":5,\"modifiers\":{\"attack_speed_mult\":1.0}}";
	private const string SwarmT2Config = "{\"trigger\":\"nearby_race_count\",\"race\":\"Goblin\",\"count_bonus\":0.06,\"count_cap\":6,\"modifiers\":{\"attack_speed_mult\":1.0}}";

	private static SkillSet CreateSwarmSet(string json)
	{
		SkillSet skills = new();
		skills.Add(new SkillDef
		{
			Id = "swarm",
			DisplayName = "Swarm",
			Type = SkillType.StatModifier,
			ConfigJson = json
		});
		return skills;
	}

	private static GameContext CreateContext(int goblinCount)
	{
		return new GameContext
		{
			Race = "Goblin",
			NearbyRaceCounts = new Dictionary<string, int> { ["Goblin"] = goblinCount }
		};
	}

	[Fact]
	public void Swarm_EachNearbyGoblin_AddsSixPercentAttackSpeed()
	{
		SkillSet skills = CreateSwarmSet(SwarmT1Config);

		Assert.Equal(1.12f, skills.GetStatMultiplier("attack_speed_mult", CreateContext(2)), 4);
	}

	[Fact]
	public void Swarm_T1_CappedAtThirtyPercent()
	{
		SkillSet skills = CreateSwarmSet(SwarmT1Config);

		Assert.Equal(1.30f, skills.GetStatMultiplier("attack_speed_mult", CreateContext(5)), 4);
		Assert.Equal(1.30f, skills.GetStatMultiplier("attack_speed_mult", CreateContext(9)), 4);
	}

	[Fact]
	public void Swarm_T2_CappedAtThirtySixPercent()
	{
		SkillSet skills = CreateSwarmSet(SwarmT2Config);

		Assert.Equal(1.36f, skills.GetStatMultiplier("attack_speed_mult", CreateContext(6)), 4);
		Assert.Equal(1.36f, skills.GetStatMultiplier("attack_speed_mult", CreateContext(9)), 4);
	}

	[Fact]
	public void Swarm_NoNearbyGoblins_NoBonus()
	{
		SkillSet skills = CreateSwarmSet(SwarmT1Config);

		Assert.Equal(1f, skills.GetStatMultiplier("attack_speed_mult", CreateContext(0)), 4);
	}
}

public class CentaurKitingTests
{
	private static SkillDef CreateKitingSkill()
	{
		return new SkillDef
		{
			Id = "kiting",
			DisplayName = "Kiting",
			Type = SkillType.Special,
			ConfigJson = "{\"behavior\":\"kiting\",\"param\":2.0,\"speed_mult\":1.3}"
		};
	}

	private static Unit CreateHunter(out Unit target)
	{
		target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		Unit hunter = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		hunter.Skills.Add(CreateKitingSkill());
		target.SetPosition(120, 0);
		hunter.SetPosition(0, 0);
		hunter.SetEnemyTarget(target);
		hunter.UpdateTargeting(target);
		return hunter;
	}

	[Fact]
	public void Kiting_AfterAttack_MoveSpeedBoosted()
	{
		Unit target;
		Unit hunter = CreateHunter(out target);

		hunter.UpdateBehavior(2f, 0, 0);

		Assert.Equal(hunter.Speed * 1.3f, hunter.GetCurrentSpeed(), 4);
	}

	[Fact]
	public void Kiting_BuffExpiresAfterDuration()
	{
		Unit target;
		Unit hunter = CreateHunter(out target);
		hunter.UpdateBehavior(2f, 0, 0);
		target.TakeDamage(999, hunter, 0, 0);

		hunter.UpdateBehavior(2.1f, 0, 0);

		Assert.Equal(hunter.Speed, hunter.GetCurrentSpeed(), 4);
	}

	[Fact]
	public void Kiting_AttackRefreshesBuff()
	{
		Unit target;
		Unit hunter = CreateHunter(out target);
		hunter.UpdateBehavior(2f, 0, 0);
		hunter.UpdateBehavior(1.5f, 0, 0);
		hunter.UpdateBehavior(2f, 0, 0);

		hunter.UpdateBehavior(1.5f, 0, 0);

		Assert.Equal(hunter.Speed * 1.3f, hunter.GetCurrentSpeed(), 4);
	}
}

public class WildHeartTests
{
	private static SkillDef CreateWildHeartSkill()
	{
		return new SkillDef
		{
			Id = "wild_heart",
			DisplayName = "Wild Heart",
			Type = SkillType.Special,
			ConfigJson = "{\"behavior\":\"regen\",\"param\":3.0}"
		};
	}

	private static Unit CreateDruid()
	{
		Unit druid = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		druid.Skills.Add(CreateWildHeartSkill());
		druid.TakeDamage(50, null, 0, 0);
		return druid;
	}

	[Fact]
	public void WildHeart_OutOfCombatThreeSeconds_RegeneratesTwoPercentPerSecond()
	{
		Unit druid = CreateDruid();

		druid.UpdateBehavior(3.0f, 0, 0);
		druid.UpdateBehavior(1.0f, 0, 0);

		Assert.Equal(58, druid.Health);
	}

	[Fact]
	public void WildHeart_CombatResetsRegenTimer()
	{
		Unit druid = CreateDruid();
		druid.UpdateBehavior(2.9f, 0, 0);
		Assert.Equal(50, druid.Health);

		druid.TakeDamage(5, null, 0, 0);
		Assert.Equal(45, druid.Health);

		druid.UpdateBehavior(2.9f, 0, 0);
		Assert.Equal(45, druid.Health);

		druid.UpdateBehavior(1.0f, 0, 0);
		Assert.Equal(47, druid.Health);
	}

	[Fact]
	public void WildHeart_FullHealth_DoesNotAccumulateRegen()
	{
		Unit druid = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		druid.Skills.Add(CreateWildHeartSkill());

		druid.UpdateBehavior(5.0f, 0, 0);

		Assert.Equal(100, druid.Health);
	}
}

public class LizardAmbushTests
{
	private static SkillDef CreateAmbushSkill(float damageMult)
	{
		string mult = damageMult.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture);
		return new SkillDef
		{
			Id = "ambush",
			DisplayName = "Ambush",
			Type = SkillType.Special,
			ConfigJson = "{\"behavior\":\"ambush\",\"param\":2.0,\"damage_mult\":" + mult + "}"
		};
	}

	private static Unit CreateHunter(float damageMult, out Unit target)
	{
		target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		Unit hunter = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		hunter.Skills.Add(CreateAmbushSkill(damageMult));
		hunter.UpdateBehavior(2.0f, 0, 0);
		target.SetPosition(120, 0);
		hunter.SetPosition(0, 0);
		hunter.SetEnemyTarget(target);
		hunter.UpdateTargeting(target);
		return hunter;
	}

	[Fact]
	public void Ambush_FirstAttackAfterTwoSecondsOutOfCombat_DealsDoubleDamage()
	{
		Unit target;
		Unit hunter = CreateHunter(2.0f, out target);

		hunter.UpdateBehavior(2.0f, 0, 0);

		Assert.Equal(60, target.Health);
	}

	[Fact]
	public void Ambush_SecondAttack_DealsNormalDamage()
	{
		Unit target;
		Unit hunter = CreateHunter(2.0f, out target);
		hunter.UpdateBehavior(2.0f, 0, 0);

		hunter.UpdateBehavior(1.0f, 0, 0);

		Assert.Equal(40, target.Health);
	}

	[Fact]
	public void Ambush_T2Stalker_FirstAttackDealsTwoAndHalfTimesDamage()
	{
		Unit target;
		Unit hunter = CreateHunter(2.5f, out target);

		hunter.UpdateBehavior(2.0f, 0, 0);

		Assert.Equal(50, target.Health);
	}

	[Fact]
	public void Ambush_TakingDamage_ClearsAmbushState()
	{
		Unit hunter = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		hunter.Skills.Add(CreateAmbushSkill(2.0f));
		hunter.UpdateBehavior(2.0f, 0, 0);
		Assert.True(hunter.IsAmbushing);

		hunter.TakeDamage(5, null, 0, 0);

		Assert.False(hunter.IsAmbushing);
	}
}

public class GoblinExplosionTests
{
	private const string ExplosionConfig = "{\"effects\":[{\"type\":\"explosion\",\"value\":12,\"range\":80}]}";

	private static Unit CreateDemolitionist(FakeUnitRepository repo, out Unit primary, out Unit bystander)
	{
		primary = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		primary.SetPosition(120, 0);
		bystander = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		bystander.SetPosition(160, 0);
		repo.Register(primary);
		repo.Register(bystander);
		Unit demolitionist = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		demolitionist.Skills.Add(new SkillDef
		{
			Id = "explosion",
			DisplayName = "Explosion",
			Type = SkillType.OnHit,
			ConfigJson = ExplosionConfig
		});
		demolitionist.Tactical = new TacticalQueryService(repo);
		demolitionist.SetPosition(0, 0);
		demolitionist.SetEnemyTarget(primary);
		demolitionist.UpdateTargeting(primary);
		repo.Register(demolitionist);
		return demolitionist;
	}

	[Fact]
	public void Explosion_Attack_DamagesNearbyEnemies()
	{
		FakeUnitRepository repo = new();
		Unit primary;
		Unit bystander;
		Unit demolitionist = CreateDemolitionist(repo, out primary, out bystander);

		demolitionist.UpdateBehavior(2f, 0, 0);

		Assert.Equal(80, primary.Health);
		Assert.Equal(88, bystander.Health);
	}

	[Fact]
	public void Explosion_Attack_DoesNotHitPrimaryTargetTwice()
	{
		FakeUnitRepository repo = new();
		Unit primary;
		Unit bystander;
		Unit demolitionist = CreateDemolitionist(repo, out primary, out bystander);

		demolitionist.UpdateBehavior(2f, 0, 0);

		Assert.Equal(80, primary.Health);
	}
}

public class SlowStrikeRealBugTests
{
	private const string SlowConfig = "{\"effects\":[{\"type\":\"slow\",\"value\":0.2,\"duration\":1.5}]}";

	private static Unit CreateRaider(out Unit target)
	{
		target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		Unit raider = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		raider.Skills.Add(new SkillDef
		{
			Id = "slow_strike",
			DisplayName = "Slow Strike",
			Type = SkillType.OnHit,
			ConfigJson = SlowConfig
		});
		target.SetPosition(120, 0);
		raider.SetPosition(0, 0);
		raider.SetEnemyTarget(target);
		raider.UpdateTargeting(target);
		return raider;
	}

	[Fact]
	public void RealBug_OnHitSlow_ReducesTargetMoveSpeed()
	{
		Unit target;
		Unit raider = CreateRaider(out target);

		raider.UpdateBehavior(2f, 0, 0);

		Assert.Equal(target.Speed * 0.8f, target.GetCurrentSpeed(), 4);
	}

	[Fact]
	public void RealBug_SlowExpires_AfterDuration()
	{
		Unit target;
		Unit raider = CreateRaider(out target);
		raider.UpdateBehavior(2f, 0, 0);

		target.UpdateBehavior(1.6f, 0, 0);

		Assert.Equal(target.Speed, target.GetCurrentSpeed(), 4);
	}
}

public class SkillContextIntegrationTests
{
	private static SkillDef CreateSwarmSkill()
	{
		return new SkillDef
		{
			Id = "swarm",
			DisplayName = "Swarm",
			Type = SkillType.StatModifier,
			ConfigJson = "{\"trigger\":\"nearby_race_count\",\"race\":\"Goblin\",\"count_bonus\":0.06,\"count_cap\":5,\"modifiers\":{\"attack_speed_mult\":1.0}}"
		};
	}

	private static SkillDef CreateSwarmAuraSkill()
	{
		return new SkillDef
		{
			Id = "swarm_aura",
			DisplayName = "Swarm Aura",
			Type = SkillType.Aura,
			ConfigJson = "{\"target\":\"nearby_allies\",\"range\":150,\"race\":\"Goblin\",\"bonus_type\":\"attack_speed\",\"value\":0.12}"
		};
	}

	[Fact]
	public void UpdateContexts_SwarmAndAura_FillGameContext()
	{
		Unit goblin = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		goblin.Race = "Goblin";
		goblin.Skills.Add(CreateSwarmSkill());
		goblin.Skills.Add(CreateSwarmAuraSkill());
		Unit ally = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		ally.Race = "Goblin";
		ally.Skills.Add(CreateSwarmAuraSkill());
		ally.SetPosition(30, 0);
		Unit orc = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		orc.Race = "Orc";
		orc.SetPosition(60, 0);
		SkillService service = new(new AuraService());

		service.UpdateContexts(new List<IUnitHandle> { goblin, ally, orc });

		Assert.Equal(1, goblin.GameCtx.NearbyRaceCounts["Goblin"]);
		Assert.Equal(1.12f, goblin.GameCtx.AuraAttackSpeedMultiplier, 4);
		Assert.Equal(1.12f, orc.GameCtx.AuraAttackSpeedMultiplier, 4);
	}
}

public class ArmorAuraCombatTests
{
	[Fact]
	public void ArmorAura_EffectiveArmor_ReducesIncomingDamage()
	{
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		target.GameCtx.AuraArmorBonus = 8;
		Unit attacker = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		target.SetPosition(120, 0);
		attacker.SetPosition(0, 0);
		attacker.SetEnemyTarget(target);
		attacker.UpdateTargeting(target);

		attacker.UpdateBehavior(2f, 0, 0);

		Assert.Equal(82, target.Health);
	}

	[Fact]
	public void ArmorAura_NoAura_TakesFullDamage()
	{
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		Unit attacker = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		target.SetPosition(120, 0);
		attacker.SetPosition(0, 0);
		attacker.SetEnemyTarget(target);
		attacker.UpdateTargeting(target);

		attacker.UpdateBehavior(2f, 0, 0);

		Assert.Equal(80, target.Health);
	}
}

public class ExecuteAuraCombatTests
{
	[Fact]
	public void ExecuteAura_LowHealthEnemy_TakesBonusDamage()
	{
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		target.TakeDamage(70, null, 0, 0);
		Unit attacker = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		attacker.GameCtx.AuraExecuteDamageMultiplier = 1.2f;
		target.SetPosition(120, 0);
		attacker.SetPosition(0, 0);
		attacker.SetEnemyTarget(target);
		attacker.UpdateTargeting(target);

		attacker.UpdateBehavior(2f, 0, 0);

		Assert.Equal(6, target.Health);
	}

	[Fact]
	public void ExecuteAura_FullHealthEnemy_DealsNormalDamage()
	{
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		Unit attacker = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		attacker.GameCtx.AuraExecuteDamageMultiplier = 1.2f;
		target.SetPosition(120, 0);
		attacker.SetPosition(0, 0);
		attacker.SetEnemyTarget(target);
		attacker.UpdateTargeting(target);

		attacker.UpdateBehavior(2f, 0, 0);

		Assert.Equal(80, target.Health);
	}
}