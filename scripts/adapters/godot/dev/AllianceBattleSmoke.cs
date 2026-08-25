// 联盟派系战斗冒烟：真实对局验证夜晚产兵与兵营升级链。
using System.Collections.Generic;
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Godot;

public partial class AllianceBattleSmoke : SmokeTestBase
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
		UpgradeBarracks,
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
		AdapterRegistry.Resolve<GameManager>().SetSelectedFaction(FactionIds.Alliance);
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
			case SmokeStep.UpgradeBarracks:
				UpgradeBarracks();
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
		TryPlaceBuilding(system, "Barracks", 0, 0);
		TryPlaceBuilding(system, "ShieldCamp", 2, 0);
		TryPlaceBuilding(system, "RangerGuild", 0, 2);
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
		_step = SmokeStep.UpgradeBarracks;
	}

	private void UpgradeBarracks()
	{
		TryUpgradeBuilding("Barracks", "Armory");
		_stepStartedAt = _elapsed;
		_step = SmokeStep.WaitUpgradedSpawns;
	}

	private void WaitUpgradedSpawns()
	{
		if (_elapsed - _stepStartedAt < UpgradeSpawnWait)
			return;
		CheckUpgradedSpawns();
		GD.Print("SMOKE: alliance battle ok");
		ReportAndQuit();
	}

	private void EnableFastProduction()
	{
		FindBuilding("Barracks")?.SetWorkSpeedMultiplier(WorkSpeedBoost);
		FindBuilding("ShieldCamp")?.SetWorkSpeedMultiplier(WorkSpeedBoost);
		FindBuilding("RangerGuild")?.SetWorkSpeedMultiplier(WorkSpeedBoost);
		FindBuilding("Armory")?.SetWorkSpeedMultiplier(WorkSpeedBoost);
	}

	private void CheckNightSpawns()
	{
		bool hasSpearman = false;
		bool hasShieldBearer = false;
		bool hasElfRanger = false;
		foreach (IUnitHandle unit in GameManager.Get<IFieldUnitRepository>().AllUnits)
		{
			if (!unit.IsAlive || !unit.IsPlayerUnit)
				continue;
			if (unit.TypeId == "Spearman")
				hasSpearman = true;
			if (unit.TypeId == "ShieldBearer")
				hasShieldBearer = true;
			if (unit.TypeId == "ElfRanger")
				hasElfRanger = true;
		}
		if (!hasSpearman)
			Fail("夜晚兵营未产出长矛兵");
		if (!hasShieldBearer)
			Fail("夜晚盾营未产出盾卫");
		if (!hasElfRanger)
			Fail("夜晚游侠公会未产出精灵游侠");
		GD.Print("SMOKE: alliance night spawns checked");
	}

	private void CheckUpgradedSpawns()
	{
		bool hasSwordsman = false;
		foreach (IUnitHandle unit in GameManager.Get<IFieldUnitRepository>().AllUnits)
		{
			if (!unit.IsAlive || !unit.IsPlayerUnit)
				continue;
			if (unit.TypeId == "Swordsman")
				hasSwordsman = true;
		}
		if (!hasSwordsman)
			Fail("军府未产出剑士");
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
		IBuildingState upgraded = useCase.TryUpgrade(
			source.Bridge, resultTypeId, healthRatio,
			AdapterRegistry.Resolve<Shop>().TrySpendGold,
			new UpgradeBuildingFactory(_playerCastle),
			HordeCoreRules.GetCoreLevel(states));
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
