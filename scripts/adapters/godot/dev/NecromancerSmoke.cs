// 亡灵派系 v0.4 无头冒烟测试：真实数据库校验光环、施法、仪式与组合门槛全链路。
using System.Collections.Generic;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using Godot;

public partial class NecromancerSmoke : Node
{
	private readonly List<string> _failures = new();

	public override void _Ready()
	{
		CheckNecromancerData();
		CheckDarkMagicData();
		CheckFanaticalAura();
		CheckSpellCasting();
		CheckRitualRules();
		CheckRitualService();
		CheckNewRituals();
		CheckCombineGate();
		CheckV05Data();
		CheckV05Auras();
		CheckV05Rituals();
		CheckV05Gates();
		ReportAndQuit();
	}

	private void CheckNecromancerData()
	{
		CheckDeathTempleData();
		CheckDeathKnightData();
		GD.Print("SMOKE: necromancer data ok");
	}

	private void CheckDeathTempleData()
	{
		IBuildingRepository buildings = GameManager.Get<IBuildingRepository>();
		BuildingData temple = buildings.Get("DeathTemple");
		if (temple.Value != 12 || temple.CombineTier != 2 || temple.UnitTypeId != "DeathKnight")
			_failures.Add("DeathTemple 数据不符");
		if (buildings.GetFactionId("DeathTemple") != FactionDef.NecromancerId)
			_failures.Add("DeathTemple 阵营错误");
		if (!buildings.GetHasNightCombat("DeathTemple"))
			_failures.Add("DeathTemple 缺少夜战标记");
	}

	private void CheckDeathKnightData()
	{
		UnitStats knight = GameManager.Get<IUnitRepository>().Get("DeathKnight");
		if (knight.Race != "Undead" || !knight.HasNightCombat || knight.Damage != 26 || knight.Health != 320)
			_failures.Add("DeathKnight 数据不符");
		IReadOnlyList<SkillDef> skills = GameManager.Get<ISkillRepository>().GetByUnitType("DeathKnight");
		if (!HasSkill(skills, "fanatical_aura"))
			_failures.Add("DeathKnight 缺少狂热光环");
	}

	private void CheckDarkMagicData()
	{
		CheckDarkAltarData();
		CheckDarkMageData();
		GD.Print("SMOKE: dark magic data ok");
	}

	private void CheckDarkAltarData()
	{
		IBuildingRepository buildings = GameManager.Get<IBuildingRepository>();
		BuildingData altar = buildings.Get("DarkAltar");
		BuildingData tower = buildings.Get("DarkMageTower");
		if (altar.Value != 3 || altar.UnitTypeId != "DarkMage")
			_failures.Add("DarkAltar 数据不符");
		if (tower.Value != 8 || tower.CombineTier != 1 || tower.UnitTypeId != "DarkMage")
			_failures.Add("DarkMageTower 数据不符");
	}

	private void CheckDarkMageData()
	{
		UnitStats mage = GameManager.Get<IUnitRepository>().Get("DarkMage");
		if (mage.Race != "Human" || !mage.HasNightCombat || mage.AttackType != AttackType.Ranged
			|| mage.DamageType != DamageType.Magic || mage.Damage != 12)
			_failures.Add("DarkMage 数据不符");
		IReadOnlyList<SkillDef> skills = GameManager.Get<ISkillRepository>().GetByUnitType("DarkMage");
		if (!HasSkill(skills, "shadow_curse") || !HasSkill(skills, "shadow_nova"))
			_failures.Add("DarkMage 技能缺失");
	}

	private void CheckFanaticalAura()
	{
		CasualCastle.Domain.Battle.Unit knight = CreateUnit(GameManager.Get<IUnitRepository>().Get("DeathKnight"), true);
		AddSkills(knight, GameManager.Get<ISkillRepository>().GetByUnitType("DeathKnight"));
		CasualCastle.Domain.Battle.Unit skeleton = CreateUnit(GameManager.Get<IUnitRepository>().Get("Skeleton"), true);
		CasualCastle.Domain.Battle.Unit human = CreateUnit(GameManager.Get<IUnitRepository>().Get("Archer"), true);
		knight.SetPosition(0, 0);
		skeleton.SetPosition(30, 0);
		human.SetPosition(-30, 0);

		AuraService service = GameManager.Get<AuraService>();
		AuraEffects undead = service.CalculateEffects(skeleton, new List<IUnitHandle> { skeleton, knight });
		if (undead.AttackDamageMultiplier < 1.19f || undead.MoveSpeedMultiplier < 1.14f)
			_failures.Add($"狂热光环对亡灵加成错误 {undead.AttackDamageMultiplier}/{undead.MoveSpeedMultiplier}");
		AuraEffects humanEffects = service.CalculateEffects(human, new List<IUnitHandle> { human, knight });
		if (humanEffects.AttackDamageMultiplier != 1f || humanEffects.MoveSpeedMultiplier != 1f)
			_failures.Add("狂热光环影响非亡灵");
		GD.Print("SMOKE: fanatical aura ok");
	}

	private void CheckSpellCasting()
	{
		CheckShadowCurse();
		CheckInsufficientBlackMagic();
		CheckShadowNova();
		GD.Print("SMOKE: spell casting ok");
	}

	private void CheckShadowCurse()
	{
		CasualCastle.Domain.Battle.Unit mage = CreateDarkMage();
		BlackMagicService blackMagic = new();
		blackMagic.Add(20);
		mage.BlackMagic = blackMagic;
		CasualCastle.Domain.Battle.Unit target = CreateTarget();
		PrepareFight(mage, target);
		mage.UpdateBehavior(2f, 0, 0);
		if (blackMagic.Value != 0)
			_failures.Add("暗影诅咒未消耗黑魔法");
		if (target.Health != 100 || target.GetCurrentSpeed() > target.Speed * 0.8f)
			_failures.Add($"暗影诅咒效果错误 health={target.Health} speed={target.GetCurrentSpeed()}");
	}

	private void CheckInsufficientBlackMagic()
	{
		CasualCastle.Domain.Battle.Unit mage = CreateDarkMage();
		BlackMagicService blackMagic = new();
		mage.BlackMagic = blackMagic;
		CasualCastle.Domain.Battle.Unit target = CreateTarget();
		PrepareFight(mage, target);
		mage.UpdateBehavior(2f, 0, 0);
		if (target.Health != 82)
			_failures.Add($"黑魔法不足应普攻 health={target.Health}");
	}

	private void CheckShadowNova()
	{
		CasualCastle.Domain.Battle.Unit mage = CreateDarkMage();
		BlackMagicService blackMagic = new();
		blackMagic.Add(40);
		mage.BlackMagic = blackMagic;
		CasualCastle.Domain.Battle.Unit target = CreateTarget();
		CasualCastle.Domain.Battle.Unit nearby = CreateTarget();
		PrepareFight(mage, target, nearby);
		mage.UpdateBehavior(2f, 0, 0);
		if (blackMagic.Value != 0)
			_failures.Add("暗影新星未消耗黑魔法");
		if (target.Health != 70 || nearby.Health != 70)
			_failures.Add($"暗影新星伤害错误 {target.Health}/{nearby.Health}");
	}
	private void CheckRitualRules()
	{
		if (!RitualRules.CanHostRitual("DarkAltar") || !RitualRules.CanHostRitual("RitualCircleL1"))
			_failures.Add("仪式宿主判定错误");
		if (RitualRules.CanHostRitual("Tomb"))
			_failures.Add("普通建筑不应可举行仪式");
		if (RitualRules.GetAll().Count != 11)
			_failures.Add("仪式定义数量错误");
		GD.Print("SMOKE: ritual rules ok");
	}

	private void CheckRitualService()
	{
		CheckRitualPrerequisites();
		CheckAltarUnlock();
		CheckDarkMagicStudy();
		CheckRitualRewards();
		GD.Print("SMOKE: ritual service ok");
	}

	private void CheckRitualPrerequisites()
	{
		RitualService service = GameManager.Get<RitualService>();
		bool goldSpent = false;
		bool magicSpent = false;
		service.Learn(RitualRules.BoneReinforceId);
		ActiveRitual blocked = service.TryStartRitual(RitualRules.BoneReinforceId, true, 1,
			_ => goldSpent = true, _ => magicSpent = true);
		if (blocked != null || goldSpent || magicSpent)
			_failures.Add("骸骨强化核心等级前置错误");
		CheckBlackMagicCharge();
	}

	private void CheckAltarUnlock()
	{
		RitualService service = GameManager.Get<RitualService>();
		if (service.IsDarkAltarUnlocked)
			_failures.Add("初始不应解锁黑暗祭坛");
		if (RitualRules.CanHostRitual("DarkAltar", false))
			_failures.Add("未研习邪恶仪式时黑暗祭坛不应主持仪式");
		if (!RitualRules.CanHostRitual("DarkAltar", true))
			_failures.Add("研习邪恶仪式后黑暗祭坛应主持仪式");
		if (!RitualRules.CanHostRitual("RitualCircleL1", false))
			_failures.Add("仪式之环不应受祭坛解锁影响");
		service.Learn(RitualRules.EvilRitualId);
		if (!service.IsDarkAltarUnlocked)
			_failures.Add("研习邪恶仪式后应解锁黑暗祭坛");
		GD.Print("SMOKE: altar unlock ok");
	}

	private void CheckBlackMagicCharge()
	{
		RitualService service = GameManager.Get<RitualService>();
		service.Learn(RitualRules.BlackMagicChargeId);
		ActiveRitual ritual = service.TryStartRitual(RitualRules.BlackMagicChargeId, true, 1,
			_ => true, _ => true);
		if (ritual == null)
		{
			_failures.Add("黑魔法充能无法开始");
			return;
		}
		service.Tick(ritual, 10f);
		if (!ritual.IsComplete)
			_failures.Add("仪式时长未完成");
		RitualReward reward = service.GetReward(ritual);
		if (reward.GoldAmount != 30)
			_failures.Add("黑魔法充能奖励错误");
	}
	private void CheckDarkMagicStudy()
	{
		RitualService service = GameManager.Get<RitualService>();
		ActiveRitual ritual = service.TryStartRitual(RitualRules.DarkMagicStudyId, true, 1,
			_ => true, _ => true, "book_smoke", RitualRules.UndeadSummonId);
		if (ritual == null)
		{
			_failures.Add("黑魔法研习无法开始");
			return;
		}
		RitualReward reward = service.Complete(ritual);
		if (reward.LearnedRitualId != RitualRules.UndeadSummonId || !service.IsLearned(RitualRules.UndeadSummonId))
			_failures.Add("黑魔法研习未习得仪式");
		if (service.StudyCompletionCount != 1)
			_failures.Add("黑魔法研习完成次数错误");
	}
	private void CheckRitualRewards()
	{
		RitualService service = GameManager.Get<RitualService>();
		service.Learn(RitualRules.UndeadSummonId);
		ActiveRitual summon = service.TryStartRitual(RitualRules.UndeadSummonId, true, 1, _ => true, _ => true);
		RitualReward coreReward = service.GetReward(summon);
		if (coreReward.SummonCount != 5 || coreReward.SummonUnitTypeId != "Skeleton")
			_failures.Add("亡灵召唤核心奖励错误");
		ActiveRitual summonBasic = service.TryStartRitual(RitualRules.UndeadSummonId, false, 1, _ => true, _ => true);
		if (service.GetReward(summonBasic).SummonCount != 3)
			_failures.Add("亡灵召唤基本奖励错误");
		ActiveRitual reinforce = service.TryStartRitual(RitualRules.BoneReinforceId, true, 2, _ => true, _ => true);
		if (service.GetReward(reinforce).UndeadMaxHealthBonus != 10)
			_failures.Add("骸骨强化核心奖励错误");
	}
	private void CheckCombineGate()
	{
		CheckCombineBlocked();
		CheckCombineAllowed();
		GD.Print("SMOKE: combine gate ok");
	}

	private void CheckCombineBlocked()
	{
		CombineService service = GameManager.Get<ICombineUseCase>() as CombineService;
		if (service == null)
		{
			_failures.Add("CombineService 未注册");
			return;
		}
		List<IBuildingState> buildings = new() { new FakeBuildingState("Necropolis", 4, 4), new FakeBuildingState("BlightMarsh", 4, 6) };
		FakeCombineFactory factory = new();
		service.ResolveCombines(buildings, true, true, true, factory, _ => { }, 2);
		if (factory.CreatedCount != 0)
			_failures.Add("核心 Lv2 不应组合死亡圣殿");
	}

	private void CheckCombineAllowed()
	{
		CombineService service = GameManager.Get<ICombineUseCase>() as CombineService;
		if (service == null)
			return;
		List<IBuildingState> buildings = new() { new FakeBuildingState("Necropolis", 4, 4), new FakeBuildingState("BlightMarsh", 4, 6) };
		FakeCombineFactory factory = new();
		service.ResolveCombines(buildings, true, true, true, factory, _ => { }, 3);
		if (factory.CreatedCount != 1 || factory.LastCreated.TypeId != "DeathTemple")
			_failures.Add("核心 Lv3 应组合出死亡圣殿");
	}
	private void CheckV05Data()
	{
		CheckBloodAltarData();
		CheckV05Units();
		CheckV05Skills();
		GD.Print("SMOKE: v0.5 data ok");
	}

	private void CheckBloodAltarData()
	{
		IBuildingRepository buildings = GameManager.Get<IBuildingRepository>();
		BuildingData altar = buildings.Get("BloodAltar");
		BuildingData sanctum = buildings.Get("NecromancySanctum");
		BuildingData gate = buildings.Get("DeathGate");
		if (altar.Value != 28 || altar.CombineTier != 3 || altar.UnitTypeId != "BloodKnight")
			_failures.Add("BloodAltar 数据不符");
		if (sanctum.Value != 65 || sanctum.CombineTier != 4 || sanctum.UnitTypeId != "NecromancyInstructor")
			_failures.Add("NecromancySanctum 数据不符");
		if (gate.Value != 80 || gate.CombineTier != 4 || gate.UnitTypeId != "DeathLord")
			_failures.Add("DeathGate 数据不符");
	}

	private void CheckV05Units()
	{
		IUnitRepository units = GameManager.Get<IUnitRepository>();
		UnitStats knight = units.Get("BloodKnight");
		if (knight.Race != "Undead" || !knight.HasNightCombat || knight.Damage != 32 || knight.Health != 620)
			_failures.Add("BloodKnight 数据不符");
		UnitStats instructor = units.Get("NecromancyInstructor");
		if (instructor.Race != "Undead" || instructor.Damage != 38 || instructor.Health != 780)
			_failures.Add("NecromancyInstructor 数据不符");
		UnitStats lord = units.Get("DeathLord");
		if (lord.Race != "Undead" || lord.Damage != 45 || lord.Health != 950)
			_failures.Add("DeathLord 数据不符");
		UnitStats angel = units.Get("DeathAngel");
		if (angel.Race != "Undead" || angel.DamageType != DamageType.Pierce || angel.Damage != 42 || angel.Health != 700)
			_failures.Add("DeathAngel 数据不符");
	}

	private void CheckV05Skills()
	{
		ISkillRepository skills = GameManager.Get<ISkillRepository>();
		if (!HasSkill(skills.GetByUnitType("BloodKnight"), "lifesteal_aura"))
			_failures.Add("BloodKnight 缺少吸血光环");
		if (!HasSkill(skills.GetByUnitType("NecromancyInstructor"), "anti_magic_shield"))
			_failures.Add("NecromancyInstructor 缺少反魔法护罩");
		if (!HasSkill(skills.GetByUnitType("DeathLord"), "stability_aura"))
			_failures.Add("DeathLord 缺少稳定光环");
	}

	private void CheckV05Auras()
	{
		CheckLifestealAura();
		CheckDispelResistAuras();
		GD.Print("SMOKE: v0.5 auras ok");
	}

	private void CheckLifestealAura()
	{
		CasualCastle.Domain.Battle.Unit knight = CreateUnit(GameManager.Get<IUnitRepository>().Get("BloodKnight"), true);
		AddSkills(knight, GameManager.Get<ISkillRepository>().GetByUnitType("BloodKnight"));
		CasualCastle.Domain.Battle.Unit skeleton = CreateUnit(GameManager.Get<IUnitRepository>().Get("Skeleton"), true);
		knight.SetPosition(0, 0);
		skeleton.SetPosition(30, 0);
		AuraService service = GameManager.Get<AuraService>();
		AuraEffects effects = service.CalculateEffects(skeleton, new List<IUnitHandle> { skeleton, knight });
		if (effects.LifestealFraction < 0.14f)
			_failures.Add($"吸血光环加成错误 {effects.LifestealFraction}");
	}

	private void CheckDispelResistAuras()
	{
		CasualCastle.Domain.Battle.Unit shield = CreateUnit(GameManager.Get<IUnitRepository>().Get("NecromancyInstructor"), true);
		AddSkills(shield, GameManager.Get<ISkillRepository>().GetByUnitType("NecromancyInstructor"));
		CasualCastle.Domain.Battle.Unit stability = CreateUnit(GameManager.Get<IUnitRepository>().Get("DeathLord"), true);
		AddSkills(stability, GameManager.Get<ISkillRepository>().GetByUnitType("DeathLord"));
		CasualCastle.Domain.Battle.Unit skeleton = CreateUnit(GameManager.Get<IUnitRepository>().Get("Skeleton"), true);
		shield.SetPosition(0, 0);
		stability.SetPosition(0, 0);
		skeleton.SetPosition(30, 0);
		AuraService service = GameManager.Get<AuraService>();
		AuraEffects shielded = service.CalculateEffects(skeleton, new List<IUnitHandle> { skeleton, shield });
		AuraEffects immune = service.CalculateEffects(skeleton, new List<IUnitHandle> { skeleton, stability });
		if (shielded.DispelResistFraction < 0.49f)
			_failures.Add($"反魔法护罩加成错误 {shielded.DispelResistFraction}");
		if (immune.DispelResistFraction < 0.99f)
			_failures.Add($"稳定光环加成错误 {immune.DispelResistFraction}");
	}

	private void CheckNewRituals()
	{
		RitualService service = GameManager.Get<RitualService>();
		service.Learn(RitualRules.ShadowContractId);
		ActiveRitual contract = service.TryStartRitual(RitualRules.ShadowContractId, true, 2, _ => true, _ => true);
		RitualReward contractReward = contract == null ? new RitualReward() : service.GetReward(contract);
		if (contractReward.BlackMagicAmount != 60)
			_failures.Add("暗影契约核心奖励错误");
		service.Learn(RitualRules.UndeadLegionId);
		ActiveRitual legion = service.TryStartRitual(RitualRules.UndeadLegionId, false, 3, _ => true, _ => true);
		RitualReward legionReward = legion == null ? new RitualReward() : service.GetReward(legion);
		if (legionReward.SummonUnitTypeId != "Skeleton" || legionReward.SummonCount != 6)
			_failures.Add("亡灵军团基本奖励错误");
		service.Learn(RitualRules.DeathKnightSummonId);
		ActiveRitual knight = service.TryStartRitual(RitualRules.DeathKnightSummonId, false, 4, _ => true, _ => true);
		RitualReward knightReward = knight == null ? new RitualReward() : service.GetReward(knight);
		if (knightReward.SummonUnitTypeId != "DeathKnight")
			_failures.Add("死亡骑士召唤基本奖励错误");
		service.Learn(RitualRules.DeadRiteId);
		ActiveRitual rite = service.TryStartRitual(RitualRules.DeadRiteId, true, 5, _ => true, _ => true);
		RitualReward riteReward = rite == null ? new RitualReward() : service.GetReward(rite);
		if (riteReward.SummonUnitTypeId != "DeathLord" || riteReward.UndeadDamageMultiplier < 0.09f)
			_failures.Add("终焉仪式核心奖励错误");
		GD.Print("SMOKE: new rituals ok");
	}

	private void CheckV05Rituals()
	{
		CheckDeathAngelRitual();
		CheckNecromancyStudyRitual();
		GD.Print("SMOKE: v0.5 rituals ok");
	}

	private void CheckDeathAngelRitual()
	{
		RitualService service = GameManager.Get<RitualService>();
		service.Learn(RitualRules.DeathAngelSummonId);
		ActiveRitual blocked = service.TryStartRitual(RitualRules.DeathAngelSummonId, false, 2, _ => true, _ => true);
		if (blocked != null)
			_failures.Add("死亡天使降临核心等级前置错误");
		ActiveRitual ritual = service.TryStartRitual(RitualRules.DeathAngelSummonId, true, 3, _ => true, _ => true);
		if (ritual == null)
		{
			_failures.Add("死亡天使降临无法开始");
			return;
		}
		RitualReward reward = service.GetReward(ritual);
		if (reward.SummonUnitTypeId != "DeathAngel" || reward.SummonCount != 1)
			_failures.Add("死亡天使降临核心奖励错误");
	}

	private void CheckNecromancyStudyRitual()
	{
		RitualService service = GameManager.Get<RitualService>();
		service.Learn(RitualRules.NecromancyStudyId);
		ActiveRitual blocked = service.TryStartRitual(RitualRules.NecromancyStudyId, false, 3, _ => true, _ => true);
		if (blocked != null)
			_failures.Add("亡灵巫术研习核心等级前置错误");
		ActiveRitual ritual = service.TryStartRitual(RitualRules.NecromancyStudyId, true, 4, _ => true, _ => true);
		if (ritual == null)
		{
			_failures.Add("亡灵巫术研习无法开始");
			return;
		}
		RitualReward reward = service.GetReward(ritual);
		if (reward.UndeadDamageMultiplier < 0.09f)
			_failures.Add($"亡灵巫术研习核心奖励错误 {reward.UndeadDamageMultiplier}");
	}

	private void CheckV05Gates()
	{
		LoadUpgradeRecipesForSmoke();
		CheckBloodAltarUpgradeGate();
		CheckSanctumUpgradeGate();
		CheckDeathGateCombineGate();
		GD.Print("SMOKE: v0.5 gates ok");
	}

	private void LoadUpgradeRecipesForSmoke()
	{
		ITechTreeRepository techRepo = GameManager.Get<ITechTreeRepository>();
		UpgradeRules rules = GameManager.Get<UpgradeRules>();
		List<UpgradeRecipe> edges = new();
		foreach (FactionDef faction in techRepo.LoadFactions())
			edges.AddRange(techRepo.LoadUpgradeEdges(faction.Id));
		edges.AddRange(RitualCircleRules.CreateUpgradeRecipes());
		rules.LoadRecipes(edges);
	}

	private void CheckBloodAltarUpgradeGate()
	{
		IUpgradeUseCase useCase = GameManager.Get<IUpgradeUseCase>();
		FakeBuildingState source = new("DeathTemple", 4, 4);
		FakeUpgradeFactory factory = new();
		IBuildingState blocked = useCase.TryUpgrade(source, "BloodAltar", 1f, _ => true, factory, 3);
		if (blocked != null)
			_failures.Add("核心 Lv3 不应升级鲜血祭坛");
		IBuildingState upgraded = useCase.TryUpgrade(source, "BloodAltar", 1f, _ => true, factory, 4);
		if (upgraded == null || upgraded.TypeId != "BloodAltar")
			_failures.Add("核心 Lv4 应升级出鲜血祭坛");
	}

	private void CheckSanctumUpgradeGate()
	{
		IUpgradeUseCase useCase = GameManager.Get<IUpgradeUseCase>();
		FakeBuildingState source = new("BloodAltar", 4, 4);
		FakeUpgradeFactory factory = new();
		IBuildingState blocked = useCase.TryUpgrade(source, "NecromancySanctum", 1f, _ => true, factory, 4);
		if (blocked != null)
			_failures.Add("核心 Lv4 不应升级亡灵巫术禁殿");
		IBuildingState upgraded = useCase.TryUpgrade(source, "NecromancySanctum", 1f, _ => true, factory, 5);
		if (upgraded == null || upgraded.TypeId != "NecromancySanctum")
			_failures.Add("核心 Lv5 应升级出亡灵巫术禁殿");
	}

	private void CheckDeathGateCombineGate()
	{
		CombineService service = GameManager.Get<ICombineUseCase>() as CombineService;
		if (service == null)
			return;
		List<IBuildingState> buildings = new() { new FakeBuildingState("BloodAltar", 4, 4), new FakeBuildingState("DarkAltar", 4, 6) };
		FakeCombineFactory factory = new();
		service.ResolveCombines(buildings, true, true, true, factory, _ => { }, 4);
		if (factory.CreatedCount != 0)
			_failures.Add("核心 Lv4 不应组合死者之门");
		service.ResolveCombines(buildings, true, true, true, factory, _ => { }, 5);
		if (factory.CreatedCount != 1 || factory.LastCreated.TypeId != "DeathGate")
			_failures.Add("核心 Lv5 应组合出死者之门");
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

	private static CasualCastle.Domain.Battle.Unit CreateDarkMage()
	{
		CasualCastle.Domain.Battle.Unit mage = CreateUnit(GameManager.Get<IUnitRepository>().Get("DarkMage"), true);
		AddSkills(mage, GameManager.Get<ISkillRepository>().GetByUnitType("DarkMage"));
		return mage;
	}

	private static void PrepareFight(CasualCastle.Domain.Battle.Unit mage, CasualCastle.Domain.Battle.Unit target, params CasualCastle.Domain.Battle.Unit[] others)
	{
		mage.SetPosition(0, 0);
		target.SetPosition(120, 0);
		for (int i = 0; i < others.Length; i++)
			others[i].SetPosition(140 + i * 20, 0);
		mage.SetEnemyTarget(target);
		mage.UpdateTargeting(target);
		FakeTacticalQueries tactical = new();
		tactical.Enemies.Add(target);
		foreach (CasualCastle.Domain.Battle.Unit other in others)
			tactical.Enemies.Add(other);
		mage.Tactical = tactical;
	}

	private static CasualCastle.Domain.Battle.Unit CreateUnit(UnitStats stats, bool isPlayerUnit)
	{
		CasualCastle.Domain.Battle.Unit unit = new(new SmokeNavPort());
		unit.Initialize(stats, isPlayerUnit);
		unit.SetDamageMatrix(GameManager.Get<DamageMatrix>());
		return unit;
	}

	private static CasualCastle.Domain.Battle.Unit CreateTarget()
	{
		UnitStats stats = new()
		{
			TypeId = "smoke_target",
			Size = UnitSize.Medium,
			AttackType = AttackType.Melee,
			DamageType = DamageType.Blunt,
			ArmorType = ArmorType.Unarmored,
			Health = 100,
			Speed = 50,
			VisionRange = 400,
		};
		CasualCastle.Domain.Battle.Unit unit = new(new SmokeNavPort());
		unit.Initialize(stats, false);
		unit.SetDamageMatrix(GameManager.Get<DamageMatrix>());
		return unit;
	}

	private static void AddSkills(CasualCastle.Domain.Battle.Unit unit, IReadOnlyList<SkillDef> skills)
	{
		foreach (SkillDef skill in skills)
			unit.Skills.Add(skill);
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

internal sealed class SmokeNavPort : INavigationPort
{
	public void SetTarget(float gameX, float gameY)
	{
	}

	public void ConfigureRvo(float radius, float neighborDistance, float timeHorizon)
	{
	}
}

internal sealed class FakeTacticalQueries : ITacticalQueries
{
	public List<IUnitHandle> Enemies { get; } = new();

	public (IUnitHandle nearest, float edgeDist) FindNearestEnemy(IUnitHandle unit)
	{
		return (null, float.MaxValue);
	}

	public void PropagateRetaliation(IUnitHandle center, IUnitHandle attacker)
	{
	}

	public IBuildingTarget FindOverlappingBuilding(IUnitHandle unit)
	{
		return null;
	}

	public bool HasEnemyOnBuilding(IBuildingRef building)
	{
		return false;
	}

	public IReadOnlyList<IUnitHandle> FindEnemiesNear(IUnitHandle center, float radius)
	{
		List<IUnitHandle> found = new();
		float radiusSq = radius * radius;
		foreach (IUnitHandle enemy in Enemies)
		{
			float dx = enemy.GameX - center.GameX;
			float dy = enemy.GameY - center.GameY;
			if (dx * dx + dy * dy <= radiusSq)
				found.Add(enemy);
		}
		return found;
	}

	public IReadOnlyList<IUnitHandle> FindEnemiesNearPoint(IUnitHandle unit, float centerX, float centerY, float radius)
	{
		List<IUnitHandle> found = new();
		float radiusSq = radius * radius;
		foreach (IUnitHandle enemy in Enemies)
		{
			float dx = enemy.GameX - centerX;
			float dy = enemy.GameY - centerY;
			if (dx * dx + dy * dy <= radiusSq)
				found.Add(enemy);
		}
		return found;
	}

	public IUnitHandle FindNearestInjuredAlly(IUnitHandle unit, float range)
	{
		return null;
	}
}

internal sealed class FakeBuildingState : IBuildingState
{
	public string TypeId { get; }
	public int AnchorGridX { get; }
	public int AnchorGridY { get; }
	public bool ContributesToAdjacency => true;
	public int Health => 100;
	public bool IsDestroyed => false;
	public bool IsManuallyPaused => false;
	public bool IsCombineProhibited => false;
	public bool HasEnemyOnTop => false;
	public bool IsPlayerOwned => true;

	public FakeBuildingState(string typeId, int anchorX, int anchorY)
	{
		TypeId = typeId;
		AnchorGridX = anchorX;
		AnchorGridY = anchorY;
	}

	public void SetWorkSpeedMultiplier(float multiplier)
	{
	}
}

internal sealed class FakeCombineFactory : ICombineBuildingFactory
{
	public int CreatedCount;
	public int DestroyedCount;
	public IBuildingState LastCreated;

	public IBuildingState Create(string typeId, int anchorX, int anchorY)
	{
		CreatedCount++;
		LastCreated = new FakeBuildingState(typeId, anchorX, anchorY);
		return LastCreated;
	}

	public void Destroy(IBuildingState building)
	{
		DestroyedCount++;
	}
}
internal sealed class FakeUpgradeFactory : IUpgradeBuildingFactory
{
	public IBuildingState Create(string typeId, int anchorX, int anchorY, float healthRatio)
	{
		return new FakeBuildingState(typeId, anchorX, anchorY);
	}

	public void Destroy(IBuildingState building)
	{
	}
}