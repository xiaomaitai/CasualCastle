using System.Collections.Generic;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Moq;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class NecromancerV05Tests
{
	private static SkillDef CreateAuraSkill(string bonusType, float value, float range = 150f)
	{
		string valueText = value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
		string rangeText = range.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
		string json = "{\"target\":\"nearby_allies\",\"range\":" + rangeText
			+ ",\"race\":\"Undead\",\"effects\":[{\"bonus_type\":\"" + bonusType + "\",\"value\":" + valueText + "}]}";
		return new SkillDef
		{
			Id = "aura_" + bonusType,
			DisplayName = "Aura",
			Type = SkillType.Aura,
			ConfigJson = json
		};
	}

	private static Unit CreateUndeadUnit(bool isPlayerUnit)
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, isPlayerUnit);
		unit.Race = "Undead";
		return unit;
	}

	private static Mock<IBuildingState> CreateBuilding(string typeId, int x, int y)
	{
		Mock<IBuildingState> building = new();
		building.SetupGet(b => b.TypeId).Returns(typeId);
		building.SetupGet(b => b.AnchorGridX).Returns(x);
		building.SetupGet(b => b.AnchorGridY).Returns(y);
		building.SetupGet(b => b.IsDestroyed).Returns(false);
		building.SetupGet(b => b.IsManuallyPaused).Returns(false);
		building.SetupGet(b => b.IsCombineProhibited).Returns(false);
		building.SetupGet(b => b.HasEnemyOnTop).Returns(false);
		building.SetupGet(b => b.IsPlayerOwned).Returns(true);
		building.SetupGet(b => b.ContributesToAdjacency).Returns(true);
		return building;
	}

	private static Mock<IBuildingRepository> CreateNecroCombineRepo()
	{
		Mock<IBuildingRepository> repo = new();
		Dictionary<string, int> tiers = new()
		{
			{ "DarkAltar", 0 },
			{ "DeathTemple", 2 },
			{ "BloodAltar", 3 },
			{ "NecromancySanctum", 4 },
			{ "DeathGate", 4 }
		};
		repo.Setup(r => r.GetCombineTier(It.IsAny<string>())).Returns<string>(t => tiers.TryGetValue(t, out int value) ? value : 0);
		repo.Setup(r => r.GetFactionId(It.IsAny<string>())).Returns(FactionDef.NecromancerId);
		repo.Setup(r => r.IsCoreBuilding(It.IsAny<string>())).Returns(false);
		GridCellOffset[] cells = { new(0, 0), new(1, 0), new(0, 1), new(1, 1) };
		repo.Setup(r => r.GetFootprint(It.IsAny<string>())).Returns(cells);
		return repo;
	}

	private static Mock<IBuildingRepository> CreateNecroUpgradeRepo()
	{
		Mock<IBuildingRepository> repo = new();
		Dictionary<string, int> tiers = new()
		{
			{ "DeathTemple", 2 },
			{ "BloodAltar", 3 },
			{ "NecromancySanctum", 4 },
			{ "DeathGate", 4 }
		};
		repo.Setup(r => r.GetCombineTier(It.IsAny<string>())).Returns<string>(t => tiers.TryGetValue(t, out int value) ? value : 0);
		repo.Setup(r => r.GetFactionId(It.IsAny<string>())).Returns(FactionDef.NecromancerId);
		return repo;
	}

	[Fact]
	public void LifestealAura_AppliesToUndeadReceiver()
	{
		Unit receiver = CreateUndeadUnit(true);
		Unit emitter = CreateUndeadUnit(true);
		emitter.Skills.Add(CreateAuraSkill(AuraBonusTypes.Lifesteal, 0.15f));
		emitter.SetPosition(30, 0);
		AuraService service = new();

		AuraEffects effects = service.CalculateEffects(receiver, new List<IUnitHandle> { receiver, emitter });

		Assert.Equal(0.15f, effects.LifestealFraction, 4);
	}

	[Fact]
	public void LifestealAura_NonUndeadReceiver_NoEffect()
	{
		Unit receiver = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		receiver.Race = "Human";
		Unit emitter = CreateUndeadUnit(true);
		emitter.Skills.Add(CreateAuraSkill(AuraBonusTypes.Lifesteal, 0.15f));
		emitter.SetPosition(30, 0);
		AuraService service = new();

		AuraEffects effects = service.CalculateEffects(receiver, new List<IUnitHandle> { receiver, emitter });

		Assert.Equal(0f, effects.LifestealFraction, 4);
	}

	[Fact]
	public void DispelResistAura_Half_AppliesToUndeadReceiver()
	{
		Unit receiver = CreateUndeadUnit(true);
		Unit emitter = CreateUndeadUnit(true);
		emitter.Skills.Add(CreateAuraSkill(AuraBonusTypes.DispelResist, 0.50f));
		emitter.SetPosition(30, 0);
		AuraService service = new();

		AuraEffects effects = service.CalculateEffects(receiver, new List<IUnitHandle> { receiver, emitter });

		Assert.Equal(0.50f, effects.DispelResistFraction, 4);
	}

	[Fact]
	public void DispelResistAura_Full_AppliesToUndeadReceiver()
	{
		Unit receiver = CreateUndeadUnit(true);
		Unit emitter = CreateUndeadUnit(true);
		emitter.Skills.Add(CreateAuraSkill(AuraBonusTypes.DispelResist, 1.00f, 180f));
		emitter.SetPosition(30, 0);
		AuraService service = new();

		AuraEffects effects = service.CalculateEffects(receiver, new List<IUnitHandle> { receiver, emitter });

		Assert.Equal(1f, effects.DispelResistFraction, 4);
	}

	[Fact]
	public void Lifesteal_HealsAttackerByFractionOfDamage()
	{
		Unit attacker = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		attacker.Damage = 10;
		attacker.ArmorValue = 0;
		attacker.GameCtx.AuraLifestealFraction = 0.15f;
		attacker.TakeDamage(30, null, 0, 0);
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		target.ArmorValue = 0;
		target.SetPosition(120, 0);
		attacker.SetPosition(0, 0);
		attacker.SetEnemyTarget(target);
		attacker.UpdateTargeting(target);

		attacker.UpdateBehavior(2f, 0, 0);

		Assert.Equal(target.MaxHealth - 10, target.Health);
		Assert.Equal(attacker.MaxHealth - 30 + 1, attacker.Health);
	}

	[Fact]
	public void Lifesteal_NoHealWithoutAura()
	{
		Unit attacker = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		attacker.Damage = 10;
		attacker.ArmorValue = 0;
		attacker.TakeDamage(30, null, 0, 0);
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		target.ArmorValue = 0;
		target.SetPosition(120, 0);
		attacker.SetPosition(0, 0);
		attacker.SetEnemyTarget(target);
		attacker.UpdateTargeting(target);

		attacker.UpdateBehavior(2f, 0, 0);

		Assert.Equal(attacker.MaxHealth - 30, attacker.Health);
	}

	[Fact]
	public void HolyDamage_VsUndeadWithHalfDispelResist_HalfBonus()
	{
		Unit undead = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		undead.Race = "Undead";
		undead.ArmorValue = 0;
		undead.GameCtx.AuraDispelResistFraction = 0.50f;
		Unit holy = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		holy.DamageType = DamageType.Holy;
		holy.Damage = 10;
		holy.ArmorValue = 0;
		undead.SetPosition(120, 0);
		holy.SetPosition(0, 0);
		holy.SetEnemyTarget(undead);
		holy.UpdateTargeting(undead);

		holy.UpdateBehavior(2f, 0, 0);

		Assert.Equal(undead.MaxHealth - 15, undead.Health);
	}

	[Fact]
	public void HolyDamage_VsUndeadFullyImmune_NoDispelBonus()
	{
		Unit undead = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		undead.Race = "Undead";
		undead.ArmorValue = 0;
		undead.GameCtx.AuraDispelResistFraction = 1f;
		Unit holy = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		holy.DamageType = DamageType.Holy;
		holy.Damage = 10;
		holy.ArmorValue = 0;
		undead.SetPosition(120, 0);
		holy.SetPosition(0, 0);
		holy.SetEnemyTarget(undead);
		holy.UpdateTargeting(undead);

		holy.UpdateBehavior(2f, 0, 0);

		Assert.Equal(undead.MaxHealth - 10, undead.Health);
	}

	[Fact]
	public void UndeadDamageBuff_MultipliesUndeadAttackerDamage()
	{
		UndeadFactionBuffs.Reset();
		UndeadFactionBuffs.AddUndeadDamageMultiplier(0.10f);
		try
		{
			Unit attacker = CreateUndeadUnit(true);
			attacker.Damage = 10;
			attacker.ArmorValue = 0;
			attacker.GameCtx.UndeadDamageMultiplier = 1.10f;
			Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
			target.ArmorValue = 0;
			target.SetPosition(120, 0);
			attacker.SetPosition(0, 0);
			attacker.SetEnemyTarget(target);
			attacker.UpdateTargeting(target);

			attacker.UpdateBehavior(2f, 0, 0);

			Assert.Equal(target.MaxHealth - 11, target.Health);
		}
		finally
		{
			UndeadFactionBuffs.Reset();
		}
	}

	[Fact]
	public void RitualCount_IncludesNewRituals()
	{
		Assert.Equal(11, RitualRules.GetAll().Count);
	}

	[Fact]
	public void DeathAngelRitual_RequiresCoreLevelThree()
	{
		RitualService service = new();
		service.Learn(RitualRules.DeathAngelSummonId);

		ActiveRitual blocked = service.TryStartRitual(RitualRules.DeathAngelSummonId, false, 2, _ => true, _ => true);
		ActiveRitual started = service.TryStartRitual(RitualRules.DeathAngelSummonId, false, 3, _ => true, _ => true);

		Assert.Null(blocked);
		Assert.NotNull(started);
	}

	[Fact]
	public void DeathAngelRitual_SpendsBlackMagicAndRewards()
	{
		RitualService service = new();
		service.Learn(RitualRules.DeathAngelSummonId);
		int spent = 0;

		ActiveRitual ritual = service.TryStartRitual(RitualRules.DeathAngelSummonId, false, 3, _ => true, amount => { spent = amount; return true; });
		RitualReward basic = service.GetReward(ritual);
		ActiveRitual core = service.TryStartRitual(RitualRules.DeathAngelSummonId, true, 3, _ => true, _ => true);
		RitualReward coreReward = service.GetReward(core);

		Assert.Equal(80, spent);
		Assert.Equal(25f, ritual.RemainingSeconds);
		Assert.Equal("SkeletonWarrior", basic.SummonUnitTypeId);
		Assert.Equal(2, basic.SummonCount);
		Assert.Equal("DeathAngel", coreReward.SummonUnitTypeId);
		Assert.Equal(1, coreReward.SummonCount);
	}

	[Fact]
	public void NecromancyStudyRitual_RequiresCoreLevelFour()
	{
		RitualService service = new();
		service.Learn(RitualRules.NecromancyStudyId);

		ActiveRitual blocked = service.TryStartRitual(RitualRules.NecromancyStudyId, false, 3, _ => true, _ => true);
		ActiveRitual started = service.TryStartRitual(RitualRules.NecromancyStudyId, false, 4, _ => true, _ => true);

		Assert.Null(blocked);
		Assert.NotNull(started);
	}

	[Fact]
	public void NecromancyStudyRitual_CoreRewardAddsUndeadDamageMultiplier()
	{
		RitualService service = new();
		service.Learn(RitualRules.NecromancyStudyId);

		ActiveRitual basic = service.TryStartRitual(RitualRules.NecromancyStudyId, false, 4, _ => true, _ => true);
		ActiveRitual core = service.TryStartRitual(RitualRules.NecromancyStudyId, true, 4, _ => true, _ => true);
		RitualReward basicReward = service.GetReward(basic);
		RitualReward coreReward = service.GetReward(core);

		Assert.Equal("BloodKnight", basicReward.SummonUnitTypeId);
		Assert.Equal(1, basicReward.SummonCount);
		Assert.Equal(0.10f, coreReward.UndeadDamageMultiplier, 4);
	}

	[Fact]
	public void Upgrade_DeathTempleToBloodAltar_RequiresCoreLevelFour()
	{
		Mock<IBuildingRepository> repo = CreateNecroUpgradeRepo();
		UpgradeRules rules = new();
		rules.LoadRecipes(new List<UpgradeRecipe> { new() { SourceTypeId = "DeathTemple", ResultTypeId = "BloodAltar" } });
		Mock<IBuildingState> source = new();
		source.SetupGet(s => s.TypeId).Returns("DeathTemple");
		source.SetupGet(s => s.IsDestroyed).Returns(false);
		source.SetupGet(s => s.IsPlayerOwned).Returns(true);

		Assert.False(rules.CanUpgrade(source.Object, repo.Object, 3, 0));
		Assert.True(rules.CanUpgrade(source.Object, repo.Object, 4, 0));
	}

	[Fact]
	public void Upgrade_BloodAltarToNecromancySanctum_RequiresCoreLevelFive()
	{
		Mock<IBuildingRepository> repo = CreateNecroUpgradeRepo();
		UpgradeRules rules = new();
		rules.LoadRecipes(new List<UpgradeRecipe> { new() { SourceTypeId = "BloodAltar", ResultTypeId = "NecromancySanctum" } });
		Mock<IBuildingState> source = new();
		source.SetupGet(s => s.TypeId).Returns("BloodAltar");
		source.SetupGet(s => s.IsDestroyed).Returns(false);
		source.SetupGet(s => s.IsPlayerOwned).Returns(true);

		Assert.False(rules.CanUpgrade(source.Object, repo.Object, 4, 0));
		Assert.True(rules.CanUpgrade(source.Object, repo.Object, 5, 0));
	}

	[Fact]
	public void Combine_BloodAltarAndDarkAltarToDeathGate_RequiresCoreLevelFive()
	{
		Mock<IBuildingRepository> repo = CreateNecroCombineRepo();
		CombineRules rules = new();
		rules.LoadRecipes(new List<CombineRecipe>
		{
			new() { MainTypeId = "BloodAltar", MaterialTypeId = "DarkAltar", MaterialCount = 1, ResultTypeId = "DeathGate" }
		});
		CombineService service = new(repo.Object, rules);
		Mock<IBuildingState> main = CreateBuilding("BloodAltar", 4, 4);
		Mock<IBuildingState> material = CreateBuilding("DarkAltar", 4, 5);
		Mock<IBuildingState> result = CreateBuilding("DeathGate", 4, 4);
		Mock<ICombineBuildingFactory> factory = new();
		factory.Setup(f => f.Create("DeathGate", 4, 4)).Returns(result.Object);
		List<IBuildingState> buildings = new() { main.Object, material.Object };

		service.ResolveCombines(buildings, true, true, true, factory.Object, _ => { }, 4);

		Assert.Equal(2, buildings.Count);

		service.ResolveCombines(buildings, true, true, true, factory.Object, _ => { }, 5);

		Assert.Single(buildings);
	}
}