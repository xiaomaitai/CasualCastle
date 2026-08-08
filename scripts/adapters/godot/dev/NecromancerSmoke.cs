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
		CheckCombineGate();
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
			|| mage.DamageType != DamageType.Shadow || mage.Damage != 12)
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
		if (RitualRules.GetAll().Count != 3)
			_failures.Add("仪式定义数量错误");
		GD.Print("SMOKE: ritual rules ok");
	}

	private void CheckRitualService()
	{
		CheckRitualPrerequisites();
		CheckRitualRewards();
		GD.Print("SMOKE: ritual service ok");
	}

	private void CheckRitualPrerequisites()
	{
		RitualService service = GameManager.Get<RitualService>();
		bool goldSpent = false;
		bool magicSpent = false;
		ActiveRitual blocked = service.TryStartRitual(RitualRules.BoneReinforceId, true, 1,
			_ => goldSpent = true, _ => magicSpent = true);
		if (blocked != null || goldSpent || magicSpent)
			_failures.Add("骸骨强化核心等级前置错误");
		CheckBlackMagicCharge();
	}

	private void CheckBlackMagicCharge()
	{
		RitualService service = GameManager.Get<RitualService>();
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
	private void CheckRitualRewards()
	{
		RitualService service = GameManager.Get<RitualService>();
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