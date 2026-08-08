// 亡灵派系 v0.4 战斗冒烟：真实对局验证夜战产兵、仪式升级、T2 升级与死亡圣殿组合。
using System.Collections.Generic;
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using Godot;

public partial class NecromancerBattleSmoke : Node
{
	private const string MainGameScene = "res://scenes/main/main_game.tscn";
	private const float NightSpawnWait = 4f;
	private const float CombineCheckDelay = 0.5f;
	private const float TotalTimeout = 60f;
	private const float WorkSpeedBoost = 10f;

	private enum SmokeStep
	{
		WaitNight,
		WaitSpawns,
		WaitFirstRitual,
		WaitSecondRitual,
		VerifyCombine
	}

	private readonly List<string> _failures = new();
	private Castle _playerCastle;
	private Building _tomb;
	private Building _darkAltar;
	private SmokeStep _step = SmokeStep.WaitNight;
	private float _elapsed;
	private float _stepStartedAt;
	private bool _nightObserved;

	public override void _Ready()
	{
		AdapterRegistry.Resolve<GameManager>().SetSelectedFaction(FactionDef.NecromancerId);
		AddChild(GD.Load<PackedScene>(MainGameScene).Instantiate());
		_playerCastle = AdapterRegistry.Resolve<GameManager>().PlayerCastle;
		PlaceBuildings();
	}

	public override void _Process(double delta)
	{
		if (_playerCastle == null)
			return;
		_elapsed += (float)delta;
		AdvanceStep();
		if (_elapsed > TotalTimeout)
		{
			_failures.Add("冒烟超时");
			ReportAndQuit();
		}
	}

	private void AdvanceStep()
	{
		switch (_step)
		{
			case SmokeStep.WaitNight:
				WaitNight();
				break;
			case SmokeStep.WaitSpawns:
				WaitSpawns();
				break;
			case SmokeStep.WaitFirstRitual:
				WaitFirstRitual();
				break;
			case SmokeStep.WaitSecondRitual:
				WaitSecondRitual();
				break;
			case SmokeStep.VerifyCombine:
				VerifyCombine();
				break;
		}
	}

	private void PlaceBuildings()
	{
		BuildingSystem system = AdapterRegistry.Resolve<BuildingSystem>();
		if (_playerCastle == null)
		{
			_failures.Add("玩家城堡缺失");
			return;
		}
		if (_playerCastle.Heart == null || _playerCastle.Heart.TypeId != "RitualCircleL1")
			_failures.Add($"仪式之环 L1 缺失 type={_playerCastle.Heart?.TypeId}");
		_tomb = TryPlaceBuilding(system, "Tomb", 0, 0);
		TryPlaceBuilding(system, "BlightField", 2, 0);
		_darkAltar = TryPlaceBuilding(system, "DarkAltar", 0, 2);
	}

	private Building TryPlaceBuilding(BuildingSystem system, string typeId, int gridX, int gridY)
	{
		if (!system.TryPlace(_playerCastle, typeId, gridX, gridY))
		{
			_failures.Add($"{typeId} 放置失败");
			return null;
		}
		return FindBuilding(typeId);
	}

	private void WaitNight()
	{
		if (!AdapterRegistry.Resolve<IGameState>().IsCombat)
			return;
		_nightObserved = true;
		_stepStartedAt = _elapsed;
		EnableFastProduction();
		_step = SmokeStep.WaitSpawns;
	}

	private void WaitSpawns()
	{
		if (_elapsed - _stepStartedAt < NightSpawnWait)
			return;
		CheckNightSpawns();
		StartRitual(RitualRules.BlackMagicChargeId);
		_stepStartedAt = _elapsed;
		_step = SmokeStep.WaitFirstRitual;
	}

	private void WaitFirstRitual()
	{
		if (_playerCastle.Heart == null || _playerCastle.Heart.TypeId != "RitualCircleL2")
			return;
		UpgradeTierTwoBuildings();
		StartRitual(RitualRules.UndeadSummonId);
		_stepStartedAt = _elapsed;
		_step = SmokeStep.WaitSecondRitual;
	}

	private void WaitSecondRitual()
	{
		if (_playerCastle.Heart == null || _playerCastle.Heart.TypeId != "RitualCircleL3")
			return;
		AdapterRegistry.Resolve<GameManager>().AdvancePhase();
		_stepStartedAt = _elapsed;
		_step = SmokeStep.VerifyCombine;
	}

	private void VerifyCombine()
	{
		if (_elapsed - _stepStartedAt < CombineCheckDelay)
			return;
		if (FindBuilding("DeathTemple") == null)
			_failures.Add("组合阶段未生成死亡圣殿");
		ReportAndQuit();
	}

	private void EnableFastProduction()
	{
		_tomb?.SetWorkSpeedMultiplier(WorkSpeedBoost);
		_darkAltar?.SetWorkSpeedMultiplier(WorkSpeedBoost);
		FindBuilding("BlightField")?.SetWorkSpeedMultiplier(WorkSpeedBoost);
	}

	private void StartRitual(string ritualId)
	{
		BlackMagicService blackMagic = GameManager.Get<BlackMagicService>();
		if (ritualId == RitualRules.BlackMagicChargeId)
			blackMagic.Add(20);
		if (_playerCastle.Heart == null || !_playerCastle.Heart.TryStartRitual(ritualId))
		{
			_failures.Add($"仪式无法开始 {ritualId}");
			ReportAndQuit();
			return;
		}
		GD.Print($"SMOKE: ritual {ritualId} started");
	}

	private void UpgradeTierTwoBuildings()
	{
		TryUpgradeBuilding("Tomb", "Necropolis");
		TryUpgradeBuilding("BlightField", "BlightMarsh");
	}

	private void TryUpgradeBuilding(string sourceTypeId, string resultTypeId)
	{
		Building source = FindBuilding(sourceTypeId);
		if (source == null)
		{
			_failures.Add($"{sourceTypeId} 缺失无法升级");
			return;
		}
		float healthRatio = source.MaxHealth > 0 ? source.Health / (float)source.MaxHealth : 1f;
		IUpgradeUseCase useCase = GameManager.Get<IUpgradeUseCase>();
		List<IBuildingState> states = _playerCastle.GetBuildingStates();
		IBuildingState upgraded = useCase.TryUpgrade(
			source, resultTypeId, healthRatio,
			AdapterRegistry.Resolve<Shop>().TrySpendGold,
			new UpgradeBuildingFactory(_playerCastle),
			RitualCircleRules.GetCoreLevel(states));
		if (upgraded == null)
			_failures.Add($"{sourceTypeId} 升级失败");
	}

	private void CheckNightSpawns()
	{
		bool hasSkeleton = false;
		bool hasDarkMage = false;
		foreach (IUnitHandle unit in GameManager.Get<IFieldUnitRepository>().AllUnits)
		{
			if (!unit.IsAlive || !unit.IsPlayerUnit)
				continue;
			if (unit.TypeId == "Skeleton")
				hasSkeleton = true;
			if (unit.TypeId == "DarkMage")
				hasDarkMage = true;
		}
		if (!hasSkeleton)
			_failures.Add("夜晚墓穴未产出骷髅兵");
		if (!hasDarkMage)
			_failures.Add("夜晚黑暗祭坛未产出暗黑法师");
		GD.Print("SMOKE: night spawns checked");
	}

	private Building FindBuilding(string typeId)
	{
		foreach (Building building in _playerCastle.GetBuildings())
		{
			if (building.TypeId == typeId)
				return building;
		}
		return null;
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
}