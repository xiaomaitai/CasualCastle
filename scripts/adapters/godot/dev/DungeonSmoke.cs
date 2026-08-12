// 地下城派系 v0.3 无头冒烟测试：真实数据库校验魔王框架全链路
using System.Collections.Generic;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using Godot;

public partial class DungeonSmoke : Node
{
	private readonly List<string> _failures = new();

	public override void _Ready()
	{
		CheckDungeonData();
		CheckShopFilter();
		CheckCoreRules();
		CheckUpgradeGate();
		CheckComfortService();
		CheckExperienceService();
		CheckSkillPool();
		CheckDemonKingCombat();
		CheckDemonAura();
		CheckV04Data();
		CheckV04UpgradeGates();
		CheckCharmingPalaceCombine();
		CheckTrapService();
		CheckLivingBuildingRules();
		CheckV05Data();
		CheckV05Buffs();
		CheckCrystalEconomy();
		CheckSlaaneshBuffs();
		CheckManaSystem();
		CheckSkillEffects();
		CheckContentGaps();
		CheckDisplayNames();
		ReportAndQuit();
	}

	private void CheckDungeonData()
	{
		CheckBuildings();
		CheckUnits();
		CheckSkills();
		GD.Print("SMOKE: dungeon data ok");
	}

	private void CheckBuildings()
	{
		IBuildingRepository buildings = GameManager.Get<IBuildingRepository>();
		BuildingData gate = buildings.Get("DemonGate");
		if (gate.FactionId != FactionDef.DungeonId || gate.Value != 3 || gate.UnitTypeId != "Imp")
			_failures.Add("DemonGate 数据不符");
		BuildingData rift = buildings.Get("HellRift");
		if (rift.CombineTier != 1 || rift.UnitTypeId != "HellHound")
			_failures.Add("HellRift 数据不符");
		BuildingData altar = buildings.Get("AbyssAltar");
		if (altar.CombineTier != 2 || altar.UnitTypeId != "AbyssDemon")
			_failures.Add("AbyssAltar 数据不符");
		if (!buildings.IsCoreBuilding("DungeonThroneL1"))
			_failures.Add("DungeonThroneL1 应为核心建筑");
		if (!buildings.GetHasNightCombat("DemonGate"))
			_failures.Add("DemonGate 缺少夜战标记");
	}

	private void CheckUnits()
	{
		IUnitRepository units = GameManager.Get<IUnitRepository>();
		UnitStats imp = units.Get("Imp");
		if (imp.Race != "Demon" || imp.Damage != 9 || imp.Health != 40)
			_failures.Add("Imp 数据不符");
		UnitStats succubus = units.Get("Succubus");
		if (succubus.AttackType != AttackType.Ranged || succubus.DamageType != DamageType.Chaos)
			_failures.Add("Succubus 数据不符");
		UnitStats king = units.Get("DemonKing");
		if (king.Race != DemonKingRules.DemonKingRace || !king.HasNightCombat)
			_failures.Add("DemonKing 数据不符");
	}

	private void CheckSkills()
	{
		ISkillRepository skills = GameManager.Get<ISkillRepository>();
		if (!HasSkill(skills.GetByUnitType("Succubus"), "succubus_aura"))
			_failures.Add("Succubus 缺少魅惑光环");
		if (!HasSkill(skills.GetByUnitType("Nurgling"), "nurgling_poison"))
			_failures.Add("Nurgling 缺少毒击");
		foreach (string skillId in DemonKingSkillPool.KhorneSkills)
		{
			SkillDef def = skills.Get(skillId);
			if (def == null)
				_failures.Add($"魔王技能 {skillId} 缺失");
		}
	}

	private void CheckShopFilter()
	{
		ShopRules rules = GameManager.Get<ShopRules>();
		rules.SetFaction(FactionDef.DungeonId);
		CardData[] offers = rules.GenerateOffers(new System.Random(123));
		foreach (CardData offer in offers)
		{
			if (offer == null || offer.FactionId != FactionDef.DungeonId)
				_failures.Add($"地下城商店混入非本派系卡 {offer?.Name}");
		}
		GD.Print("SMOKE: shop filter ok");
	}

	private void CheckCoreRules()
	{
		if (DungeonCoreRules.GetLevel("DungeonThroneL3") != 3)
			_failures.Add("宝座等级解析错误");
		if (DungeonCoreRules.GetLevel("DemonGate") != 0)
			_failures.Add("普通建筑不应解析出宝座等级");
		if (DungeonCoreRules.CreateUpgradeRecipes().Count != 4)
			_failures.Add("宝座升级配方数量错误");
		GD.Print("SMOKE: core rules ok");
	}

	private void CheckUpgradeGate()
	{
		ITechTreeRepository techRepo = GameManager.Get<ITechTreeRepository>();
		UpgradeRules rules = GameManager.Get<UpgradeRules>();
		List<UpgradeRecipe> edges = new();
		foreach (FactionDef faction in techRepo.LoadFactions())
			edges.AddRange(techRepo.LoadUpgradeEdges(faction.Id));
		edges.AddRange(HordeCoreRules.CreateUpgradeRecipes());
		edges.AddRange(RitualCircleRules.CreateUpgradeRecipes());
		edges.AddRange(DungeonCoreRules.CreateUpgradeRecipes());
		rules.LoadRecipes(edges);

		IUpgradeUseCase useCase = GameManager.Get<IUpgradeUseCase>();
		FakeBuildingState gate = new("DemonGate", 4, 4);
		FakeUpgradeFactory factory = new();
		IBuildingState blocked = useCase.TryUpgrade(gate, "HellRift", 1f, _ => true, factory, 1);
		if (blocked != null)
			_failures.Add("魔王 Lv1 不应升级地狱裂隙");
		IBuildingState upgraded = useCase.TryUpgrade(gate, "HellRift", 1f, _ => true, factory, 2);
		if (upgraded == null || upgraded.TypeId != "HellRift")
			_failures.Add("魔王 Lv2 应升级出地狱裂隙");
		FakeBuildingState altar = new("AbyssAltar", 4, 4);
		IBuildingState blockedAltar = useCase.TryUpgrade(new FakeBuildingState("HellRift", 4, 4), "AbyssAltar", 1f, _ => true, factory, 2);
		if (blockedAltar != null)
			_failures.Add("魔王 Lv2 不应升级深渊祭坛");
		IBuildingState upgradedAltar = useCase.TryUpgrade(new FakeBuildingState("HellRift", 4, 4), "AbyssAltar", 1f, _ => true, factory, 3);
		if (upgradedAltar == null || upgradedAltar.TypeId != "AbyssAltar")
			_failures.Add("魔王 Lv3 应升级出深渊祭坛");
		GD.Print("SMOKE: upgrade gate ok");
	}

	private void CheckComfortService()
	{
		ComfortService comfort = new();
		if (comfort.Value != DemonKingRules.MaxComfort)
			_failures.Add("舒适度初始值错误");
		comfort.SpendAttack();
		if (comfort.Value != DemonKingRules.MaxComfort - DemonKingRules.AttackComfortCost)
			_failures.Add("攻击消耗错误");
		comfort.Spend(90f);
		if (comfort.Value != 5f || comfort.IsCombatReady)
			_failures.Add("舒适度耗尽判定错误");
		comfort.TickRestore(1f, true, false);
		if (comfort.Value < 10f)
			_failures.Add("宝座附近恢复错误");
		GD.Print("SMOKE: comfort service ok");
	}

	private void CheckExperienceService()
	{
		KingExperienceService xp = new();
		int levelUps = 0;
		xp.LevelUp += _ => levelUps++;
		xp.OnUnitDied(false);
		if (xp.Experience != DemonKingRules.EnemyDeathXp)
			_failures.Add("敌方阵亡经验错误");
		xp.OnKingKill();
		xp.OnCrystalUsed();
		if (xp.GetCumulativeExperience() != DemonKingRules.EnemyDeathXp + DemonKingRules.KingKillXp + DemonKingRules.CrystalXp)
			_failures.Add("累计经验错误");
		xp.AddExperience(DemonKingRules.GetRequiredExperienceForLevel(1));
		if (xp.Level != 2 || levelUps != 1)
			_failures.Add("升级流程错误");
		GD.Print("SMOKE: experience service ok");
	}

	private void CheckSkillPool()
	{
		System.Random random = new(7);
		List<string> choices = DemonKingSkillPool.GenerateChoices(random, 4, new List<string>());
		if (choices.Count != 3)
			_failures.Add("技能三选一数量错误");
		foreach (string skillId in choices)
		{
			if (DemonKingSkillPool.IsUltimate(skillId))
				_failures.Add($"Lv4 不应出现终极技能 {skillId}");
		}
		List<string> lv5 = DemonKingSkillPool.GenerateChoices(new System.Random(9), 5, new List<string>());
		if (lv5.Count != 3)
			_failures.Add("Lv5 技能抽取错误");
		GD.Print("SMOKE: skill pool ok");
	}

	private void CheckDemonKingCombat()
	{
		CasualCastle.Domain.Battle.Unit king = CreateKing();
		ComfortService comfort = new();
		king.Comfort = comfort;
		CasualCastle.Domain.Battle.Unit attacker = CreateUnit(GameManager.Get<IUnitRepository>().Get("Imp"), false);
		king.TakeDamage(999, attacker, 0f, 0f);
		if (!king.IsAlive || king.Health != 1500)
			_failures.Add("魔王无敌光环失效");
		if (comfort.Value != DemonKingRules.MaxComfort - DemonKingRules.HitComfortCost)
			_failures.Add("魔王受击未消耗舒适度");
		GD.Print("SMOKE: demon king combat ok");
	}

	private void CheckDemonAura()
	{
		CasualCastle.Domain.Battle.Unit king = CreateKing();
		king.SetPosition(0f, 0f);
		CasualCastle.Domain.Battle.Unit succubus = CreateUnit(GameManager.Get<IUnitRepository>().Get("Succubus"), true);
		succubus.Skills.Add(GameManager.Get<ISkillRepository>().Get("succubus_aura"));
		succubus.SetPosition(30f, 0f);
		AuraService service = GameManager.Get<AuraService>();
		AuraEffects effects = service.CalculateEffects(king, new List<IUnitHandle> { king, succubus });
		if (effects.AttackDamageMultiplier < 1.14f)
			_failures.Add($"魅惑光环未作用于魔王 {effects.AttackDamageMultiplier}");
		GD.Print("SMOKE: demon aura ok");
	}

	private void CheckV04Data()
	{
		CheckV04Buildings();
		CheckV04Units();
		CheckV04Skills();
		GD.Print("SMOKE: v0.4 data ok");
	}

	private void CheckV04Buildings()
	{
		IBuildingRepository buildings = GameManager.Get<IBuildingRepository>();
		BuildingData gate = buildings.Get("DestructionGate");
		if (gate.CombineTier != 3 || gate.UnitTypeId != "DoomGuard")
			_failures.Add("DestructionGate 数据不符");
		BuildingData palace = buildings.Get("CharmingPalace");
		if (palace.CombineTier != 3 || palace.UnitTypeId != "SuccubusMaid")
			_failures.Add("CharmingPalace 数据不符");
		BuildingData garden = buildings.Get("NurgleGarden");
		if (garden.CombineTier != 4 || garden.UnitTypeId != "PlagueBehemoth")
			_failures.Add("NurgleGarden 数据不符");
		if (!TrapRules.IsTrap("TeleportTrap"))
			_failures.Add("传送陷阱未识别为陷阱");
	}

	private void CheckV04Units()
	{
		IUnitRepository units = GameManager.Get<IUnitRepository>();
		UnitStats guard = units.Get("DoomGuard");
		if (guard.Race != "Demon" || guard.Damage != 34 || guard.Health != 520)
			_failures.Add("DoomGuard 数据不符");
		UnitStats behemoth = units.Get("PlagueBehemoth");
		if (behemoth.Size != UnitSize.Huge || behemoth.Health != 900)
			_failures.Add("PlagueBehemoth 数据不符");
	}

	private void CheckV04Skills()
	{
		ISkillRepository skills = GameManager.Get<ISkillRepository>();
		if (!HasSkill(skills.GetByUnitType("DoomGuard"), "doom_guard_cleave"))
			_failures.Add("DoomGuard 缺少顺劈");
		if (!HasSkill(skills.GetByUnitType("DemonScholar"), "demon_scholar_aura"))
			_failures.Add("DemonScholar 缺少鼓舞光环");
		if (!HasSkill(skills.GetByUnitType("PlagueBehemoth"), "plague_behemoth_regen"))
			_failures.Add("PlagueBehemoth 缺少再生");
		if (DemonKingSkillPool.GetNormalSkillCount() != 22)
			_failures.Add("普通技能池数量错误");
	}

	private void CheckV04UpgradeGates()
	{
		IUpgradeUseCase useCase = GameManager.Get<IUpgradeUseCase>();
		FakeUpgradeFactory factory = new();
		IBuildingState blocked = useCase.TryUpgrade(new FakeBuildingState("AbyssAltar", 4, 4), "DestructionGate", 1f, _ => true, factory, 3);
		if (blocked != null)
			_failures.Add("魔王 Lv3 不应升级毁灭之门");
		IBuildingState upgraded = useCase.TryUpgrade(new FakeBuildingState("AbyssAltar", 4, 4), "DestructionGate", 1f, _ => true, factory, 4);
		if (upgraded == null || upgraded.TypeId != "DestructionGate")
			_failures.Add("魔王 Lv4 应升级出毁灭之门");
		GD.Print("SMOKE: v0.4 upgrade gates ok");
	}

	private void CheckCharmingPalaceCombine()
	{
		CombineService service = GameManager.Get<ICombineUseCase>() as CombineService;
		if (service == null)
			return;
		List<IBuildingState> buildings = new() { new FakeBuildingState("HeartPortal", 4, 4), new FakeBuildingState("MaidResidence", 4, 6) };
		FakeCombineFactory factory = new();
		service.ResolveCombines(buildings, true, true, true, factory, _ => { }, 3);
		if (factory.CreatedCount != 0)
			_failures.Add("魔王 Lv3 不应组合魅惑宫殿");
		service.ResolveCombines(buildings, true, true, true, factory, _ => { }, 4);
		if (factory.CreatedCount != 1 || factory.LastCreated.TypeId != "CharmingPalace")
			_failures.Add("魔王 Lv4 应组合出魅惑宫殿");
		GD.Print("SMOKE: charming palace combine ok");
	}

	private void CheckTrapService()
	{
		CasualCastle.Domain.Battle.Unit source = CreateUnit(GameManager.Get<IUnitRepository>().Get("Imp"), true);
		CasualCastle.Domain.Battle.Unit target = CreateUnit(GameManager.Get<IUnitRepository>().Get("Imp"), false);
		target.SetPosition(10f, 0f);
		FakeTacticalQueries tactical = new();
		tactical.Enemies.Add(target);
		TrapService service = new(tactical, GameManager.Get<DamageMatrix>());
		int hits = service.Trigger(source, TrapRules.SpikeTrap, 0f, 0f, 0f, 0f);
		if (hits != 1 || target.Health >= 40)
			_failures.Add("尖刺陷阱未生效");
		GD.Print("SMOKE: trap service ok");
	}

	private void CheckLivingBuildingRules()
	{
		if (!LivingBuildingRules.IsLivingBuilding("FleshNest"))
			_failures.Add("血肉之巢应识别为活体建筑");
		if (LivingBuildingRules.GetNextInLine("FleshNest") != "PusPool")
			_failures.Add("血肉之巢成长链错误");
		if (LivingBuildingRules.GetRequiredKingLevel("RegrowthTree") != 4)
			_failures.Add("再生之树成长门槛错误");
		GD.Print("SMOKE: living building rules ok");
	}
	private void CheckV05Data()
	{
		IBuildingRepository buildings = GameManager.Get<IBuildingRepository>();
		BuildingData throne = buildings.Get("KhorneBloodThrone");
		if (throne.CombineTier != 4 || throne.Value != 60)
			_failures.Add("KhorneBloodThrone 数据不符");
		BuildingData labyrinth = buildings.Get("TzeentchLabyrinth");
		if (labyrinth.CombineTier != 4)
			_failures.Add("TzeentchLabyrinth 数据不符");
		BuildingData paradise = buildings.Get("SlaaneshParadise");
		if (paradise.CombineTier != 4)
			_failures.Add("SlaaneshParadise 数据不符");
		BuildingData garden = buildings.Get("NurgleGarden");
		if (garden.CombineTier != 4 || garden.UnitTypeId != "PlagueBehemoth")
			_failures.Add("NurgleGarden 数据不符");
		GD.Print("SMOKE: v0.5 data ok");
	}

	private void CheckV05Buffs()
	{
		ComfortService comfort = new();
		comfort.SetMaxComfort(DemonKingRules.MaxComfort + 50f);
		if (comfort.MaxComfort != 150f)
			_failures.Add("舒适度上限扩展失败");
		comfort.GlobalCostMultiplier = 2f;
		comfort.SpendAttack();
		if (comfort.Value != DemonKingRules.MaxComfort - DemonKingRules.AttackComfortCost * 2f)
			_failures.Add("舒适度消耗倍率失败");

		KingExperienceService xp = new();
		xp.KillXpMultiplier = 1.5f;
		xp.OnKingKill();
		if (xp.Experience != (int)(DemonKingRules.KingKillXp * 1.5f))
			_failures.Add("击杀经验倍率失败");

		DungeonFactionBuffs.GlobalRegenPerSecond = 0.01f;
		DungeonFactionBuffs.AutoBreedEnabled = true;
		DungeonFactionBuffs.Reset();
		if (DungeonFactionBuffs.GlobalRegenPerSecond != 0f || DungeonFactionBuffs.AutoBreedEnabled)
			_failures.Add("地下城增益重置失败");
		GD.Print("SMOKE: v0.5 buffs ok");
	}
	private void CheckCrystalEconomy()
	{
		IBuildingRepository buildings = GameManager.Get<IBuildingRepository>();
		BuildingData mine = buildings.Get("CrystalMine");
		if (mine.Value != 3 || mine.FactionId != FactionDef.DungeonId)
			_failures.Add("CrystalMine 数据不符");
		UnitStats bloodletter = GameManager.Get<IUnitRepository>().Get("Bloodletter");
		if (bloodletter.Race != "Demon" || bloodletter.Health != 300)
			_failures.Add("Bloodletter 数据不符");
		KingExperienceService xp = new();
		xp.OnSmallCrystalMined();
		if (xp.Experience != DemonKingRules.SmallCrystalXp)
			_failures.Add("小型魔晶经验错误");
		CardData crystalCard = null;
		foreach (CardData card in GameManager.Get<IReadOnlyList<CardData>>())
		{
			if (card.Id == "DungeonCrystal")
				crystalCard = card;
		}
		if (crystalCard == null || crystalCard.ItemType != ShopItemTypes.DungeonCrystal)
			_failures.Add("经验魔晶物品卡类型错误");
		GD.Print("SMOKE: crystal economy ok");
	}

	private void CheckSlaaneshBuffs()
	{
		List<IBuildingState> buildings = new()
		{
			new FakeBuildingState("DungeonThroneL1", 4, 4),
			new FakeBuildingState(SlaaneshServiceRules.HotSpring, 4, 2),
		};
		SlaaneshComfortBuffs buffs = SlaaneshServiceRules.CalculateComfortBuffs(buildings, GameManager.Get<IBuildingRepository>());
		if (buffs.RestoreMultiplier < 1.29f)
			_failures.Add("温泉邻接加成未生效");
		GD.Print("SMOKE: slaanesh buffs ok");
	}

	private void CheckManaSystem()
	{
		ManaService mana = new();
		mana.Add(40f);
		if (mana.Value != 40f)
			_failures.Add("魔力累积错误");
		CasualCastle.Domain.Battle.Unit king = CreateUnit(GameManager.Get<IUnitRepository>().Get("DemonKing"), true);
		king.Mana = mana;
		king.Skills.Add(GameManager.Get<ISkillRepository>().Get("mana_storm"));
		CasualCastle.Domain.Battle.Unit target = CreateUnit(GameManager.Get<IUnitRepository>().Get("Imp"), false);
		target.SetPosition(50f, 0f);
		king.SetPosition(0f, 0f);
		king.SetEnemyTarget(target);
		king.UpdateTargeting(target);
		FakeTacticalQueries tactical = new();
		tactical.Enemies.Add(target);
		king.Tactical = tactical;
		king.UpdateBehavior(2f, 0f, 0f);
		if (mana.Value != 0f || target.Health >= 40)
			_failures.Add("魔力风暴未生效");
		GD.Print("SMOKE: mana system ok");
	}
	private void CheckSkillEffects()
	{
		ISkillRepository skills = GameManager.Get<ISkillRepository>();
		SkillDef flame = skills.Get("flame_slash");
		if (flame.Type != SkillType.OnHit)
			_failures.Add("魔焰斩应为范围普攻");
		SkillDef regen = skills.Get("regen_aura_skill");
		if (regen.Type != SkillType.Aura)
			_failures.Add("再生光环应为光环");
		SkillDef life = skills.Get("life_link");
		if (life.Type != SkillType.Aura)
			_failures.Add("生命链接应为光环");
		if (TrapRules.GetMaxTrapCount(false) != 4 || TrapRules.GetMaxTrapCount(true) != 8)
			_failures.Add("陷阱上限常量错误");
		GD.Print("SMOKE: skill effects ok");
	}
	private void CheckContentGaps()
	{
		IBuildingRepository buildings = GameManager.Get<IBuildingRepository>();
		BuildingData box = buildings.Get("TreasureBox");
		if (!TrapRules.IsTrap("TreasureBox") || box.Value != 3)
			_failures.Add("宝箱陷阱数据不符");
		BuildingData stage = buildings.Get("PleasureStage");
		if (stage.UnitTypeId != "Bard")
			_failures.Add("享乐舞台未产出吟游诗人");
		BuildingData tree = buildings.Get("RegrowthTree");
		if (tree.UnitTypeId != "PlagueDoctor")
			_failures.Add("再生之树未产出瘟疫医师");
		IUnitRepository units = GameManager.Get<IUnitRepository>();
		UnitStats bard = units.Get("Bard");
		if (bard.Race != "Demon")
			_failures.Add("Bard 数据不符");
		UnitStats doctor = units.Get("PlagueDoctor");
		if (doctor.HealAmount <= 0f || doctor.HealRange <= 0f)
			_failures.Add("瘟疫医师缺少治疗参数");
		GD.Print("SMOKE: content gaps ok");
	}
	private void CheckDisplayNames()
	{
		IBuildingRepository buildings = GameManager.Get<IBuildingRepository>();
		string throne = buildings.Get("DungeonThroneL1").DisplayName;
		string garden = buildings.Get("NurgleGarden").DisplayName;
		if (!throne.Contains("魔王宝座"))
			_failures.Add($"宝座显示名错误 [{throne}]");
		if (!garden.Contains("纳垢花园"))
			_failures.Add($"纳垢花园显示名错误 [{garden}]");
		GD.Print("SMOKE: display names ok");
	}
	private void ReportAndQuit()
	{
		if (_failures.Count == 0)
		{
			GD.Print("SMOKE: PASS");
			GetTree().Quit(0);
			return;
		}

		foreach (string failure in _failures)
			GD.Print($"SMOKE: FAIL {failure}");
		GetTree().Quit(1);
	}

	private static CasualCastle.Domain.Battle.Unit CreateKing()
	{
		UnitStats stats = GameManager.Get<IUnitRepository>().Get("DemonKing");
		CasualCastle.Domain.Battle.Unit unit = new(new SmokeNavPort());
		unit.Initialize(stats, true);
		unit.SetDamageMatrix(GameManager.Get<DamageMatrix>());
		return unit;
	}

	private static CasualCastle.Domain.Battle.Unit CreateUnit(UnitStats stats, bool isPlayerUnit)
	{
		CasualCastle.Domain.Battle.Unit unit = new(new SmokeNavPort());
		unit.Initialize(stats, isPlayerUnit);
		unit.SetDamageMatrix(GameManager.Get<DamageMatrix>());
		return unit;
	}

	private static bool HasSkill(IReadOnlyList<SkillDef> skills, string skillId)
	{
		foreach (SkillDef skill in skills)
		{
			if (skill.Id == skillId)
				return true;
		}
		return false;
	}
}