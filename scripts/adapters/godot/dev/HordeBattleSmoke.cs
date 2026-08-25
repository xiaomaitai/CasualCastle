// 部落派系战斗冒烟：真实对局验证夜晚产兵与兽人营地升级链。
using System.Collections.Generic;
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Godot;

public partial class HordeBattleSmoke : SmokeTestBase
{
	private const string MainGameScene = "res://scenes/main/main_game.tscn";
	private const float NightSpawnWait = 4f;
	private const float UpgradeSpawnWait = 8f;
	private const float TotalTimeout = 60f;
	private const float WorkSpeedBoost = 10f;

	private enum SmokeStep
	{
		WaitNight,
		WaitSpawns,
		UpgradeCore,
		UpgradeGoblinHut,
		WaitUpgradedSpawns
	}

	private readonly List<string> _failures = new();
	private Castle _playerCastle;
	private SmokeStep _step = SmokeStep.WaitNight;
	private float _elapsed;
	private float _stepStartedAt;

	public override void _Ready()
	{
		StartWatchdog(240f);
		AdapterRegistry.Resolve<GameManager>().SetSelectedFaction(FactionDef.HordeId);
		AddChild(GD.Load<PackedScene>(MainGameScene).Instantiate());
		_playerCastle = AdapterRegistry.Resolve<GameManager>().PlayerCastle;
		PlaceBuildings();
	}

	public override void _Process(double delta)
	{
		if (_playerCastle == null)
			return;
		_elapsed += (float)delta;
		RunGuarded(AdvanceStep);
		if (_elapsed > TotalTimeout)
		{
			Fail("冒烟超时");
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
			case SmokeStep.UpgradeCore:
				UpgradeCore();
				break;
			case SmokeStep.UpgradeGoblinHut:
				UpgradeGoblinHut();
				break;
			case SmokeStep.WaitUpgradedSpawns:
				WaitUpgradedSpawns();
				break;
		}
	}

	private void PlaceBuildings()
	{
		if (_playerCastle == null)
		{
			Fail("玩家城堡缺失");
			return;
		}
		BuildingSystem system = AdapterRegistry.Resolve<BuildingSystem>();
		TryPlaceBuilding(system, "GoblinHut", 0, 0);
		TryPlaceBuilding(system, "OrcCamp", 2, 0);
		TryPlaceBuilding(system, "LizardNest", 0, 2);
		TryPlaceBuilding(system, "Burrow", 4, 0);
	}

	private Building TryPlaceBuilding(BuildingSystem system, string typeId, int gridX, int gridY)
	{
		if (!system.TryPlace(_playerCastle, typeId, gridX, gridY))
		{
			Fail($"{typeId} 放置失败");
			return null;
		}
		return FindBuilding(typeId);
	}

	private void WaitNight()
	{
		if (!AdapterRegistry.Resolve<IGameState>().IsCombat)
			return;
		EnableFastProduction();
		_stepStartedAt = _elapsed;
		_step = SmokeStep.WaitSpawns;
	}

	private void WaitSpawns()
	{
		if (_elapsed - _stepStartedAt < NightSpawnWait)
			return;
		CheckNightSpawns();
		_stepStartedAt = _elapsed;
		_step = SmokeStep.UpgradeCore;
	}

	private void UpgradeCore()
	{
		TryUpgradeBuilding("HordeCoreL1", "HordeCoreL2");
		_stepStartedAt = _elapsed;
		_step = SmokeStep.UpgradeGoblinHut;
	}

	private void UpgradeGoblinHut()
	{
		TryUpgradeBuilding("GoblinHut", "GoblinWorkshop");
		_stepStartedAt = _elapsed;
		_step = SmokeStep.WaitUpgradedSpawns;
	}

	private void WaitUpgradedSpawns()
	{
		if (_elapsed - _stepStartedAt < UpgradeSpawnWait)
			return;
		CheckUpgradedSpawns();
		GD.Print("SMOKE: horde battle ok");
		ReportAndQuit();
	}

	private void EnableFastProduction()
	{
		FindBuilding("GoblinHut")?.SetWorkSpeedMultiplier(WorkSpeedBoost);
		FindBuilding("OrcCamp")?.SetWorkSpeedMultiplier(WorkSpeedBoost);
		FindBuilding("LizardNest")?.SetWorkSpeedMultiplier(WorkSpeedBoost);
		FindBuilding("GoblinWorkshop")?.SetWorkSpeedMultiplier(WorkSpeedBoost);
	}

	private void CheckNightSpawns()
	{
		bool hasGoblin = false;
		bool hasOrc = false;
		bool hasLizard = false;
		foreach (IUnitHandle unit in GameManager.Get<IFieldUnitRepository>().AllUnits)
		{
			if (!unit.IsAlive || !unit.IsPlayerUnit)
				continue;
			if (unit.TypeId == "GoblinSlinger")
				hasGoblin = true;
			if (unit.TypeId == "OrcWarrior")
				hasOrc = true;
			if (unit.TypeId == "LizardHunter")
				hasLizard = true;
		}
		if (!hasGoblin)
			Fail("夜晚哥布林窝棚未产出哥布林投手");
		if (!hasOrc)
			Fail("夜晚兽人营地未产出兽人战士");
		if (!hasLizard)
			Fail("夜晚蜥蜴人巢穴未产出蜥蜴猎手");
		GD.Print("SMOKE: horde night spawns checked");
	}

	private void CheckUpgradedSpawns()
	{
		bool hasDemolitionist = false;
		foreach (IUnitHandle unit in GameManager.Get<IFieldUnitRepository>().AllUnits)
		{
			if (!unit.IsAlive || !unit.IsPlayerUnit)
				continue;
			if (unit.TypeId == "GoblinDemolitionist")
				hasDemolitionist = true;
		}
		if (!hasDemolitionist)
			Fail("哥布林工坊未产出爆破手");
	}

	private void TryUpgradeBuilding(string sourceTypeId, string resultTypeId)
	{
		Building source = FindBuilding(sourceTypeId);
		if (source == null)
		{
			Fail($"{sourceTypeId} 缺失无法升级");
			return;
		}
		float healthRatio = source.MaxHealth > 0 ? source.Health / (float)source.MaxHealth : 1f;
		IUpgradeUseCase useCase = GameManager.Get<IUpgradeUseCase>();
		List<IBuildingState> states = _playerCastle.GetBuildingStates();
		int core = HordeCoreRules.GetCoreLevel(states);
		int burrow = HordeCoreRules.CountBurrows(states);
		IBuildingState upgraded = useCase.TryUpgrade(
			source.Bridge, resultTypeId, healthRatio,
			AdapterRegistry.Resolve<Shop>().TrySpendGold,
			new UpgradeBuildingFactory(_playerCastle),
			core,
			burrow);
		if (upgraded == null)
		{
			Fail($"{sourceTypeId} 升级失败");
			return;
		}
		FindBuilding(resultTypeId)?.SetWorkSpeedMultiplier(WorkSpeedBoost);
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
}
