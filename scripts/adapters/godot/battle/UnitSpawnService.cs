// 单位生成服务：普通产兵与仪式召唤共用的装载、初始化和战场放置
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using Godot;
using System;

public sealed class UnitSpawnService
{
	private const string UnitPrefabPath = "res://prefabs/Unit.tscn";

	public void SpawnProduction(Building building, int count)
	{
		if (!building.CanWork || building.Battlefield == null || building.GetCastle() == null || count <= 0)
			return;
		if (IsGameOver())
			return;
		BuildingSystem buildingSystem = AdapterRegistry.Resolve<BuildingSystem>();
		if (buildingSystem == null)
			return;
		PackedScene unitScene = GD.Load<PackedScene>(UnitPrefabPath);
		if (unitScene == null)
			return;
		for (int i = 0; i < count; i++)
			Spawn(building, buildingSystem, unitScene, unit => buildingSystem.ApplyUnitSpawnStats(building.TypeId, unit));
	}

	public void SpawnUnitType(Building building, string unitTypeId, int count)
	{
		if (building.Battlefield == null || building.GetCastle() == null || count <= 0 || string.IsNullOrEmpty(unitTypeId))
			return;
		if (IsGameOver())
			return;
		BuildingSystem buildingSystem = AdapterRegistry.Resolve<BuildingSystem>();
		if (buildingSystem == null)
			return;
		IUnitRepository unitRepo = GameManager.Get<IUnitRepository>();
		if (unitRepo == null)
			return;
		PackedScene unitScene = GD.Load<PackedScene>(UnitPrefabPath);
		if (unitScene == null)
			return;
		for (int i = 0; i < count; i++)
			Spawn(building, buildingSystem, unitScene, unit => unit.InitializeFromStats(unitRepo.Get(unitTypeId)));
	}

	private void Spawn(Building building, BuildingSystem buildingSystem, PackedScene unitScene, Action<UnitLogic> configure)
	{
		Unit shell = unitScene.Instantiate<Unit>();
		UnitLogic unit = shell.GetNode<UnitLogic>("Logic");
		unit.IsPlayerUnit = building.IsPlayerOwned;
		configure(unit);
		ApplyFactionBuff(unit);
		UnitSpawn.PlaceUnit(
			building.Battlefield, building.GetCastle(), shell,
			buildingSystem.GetFootprint(building.TypeId), building.AnchorGridX, building.AnchorGridY, building.NextSpawnIndex());
		if (building.IsPlayerOwned)
			AssignToTavern(unit);
	}

	private static bool IsGameOver()
	{
		GameManager gameManager = AdapterRegistry.Resolve<GameManager>();
		return gameManager != null && gameManager.CurrentState == GameManager.GameState.GameOver;
	}

	private static void ApplyFactionBuff(UnitLogic unit)
	{
		if (unit.Handle != null && UndeadRules.IsUndead(unit.Handle.Race) && UndeadFactionBuffs.MaxHealthBonus > 0)
			unit.Handle.AddMaxHealthBonus(UndeadFactionBuffs.MaxHealthBonus);
	}

	private void AssignToTavern(UnitLogic unit)
	{
		TavernPartyService tavernParty = GameManager.Get<TavernPartyService>();
		IFieldUnitRepository fieldRepo = GameManager.Get<IFieldUnitRepository>();
		if (tavernParty == null || fieldRepo == null)
			return;
		tavernParty.TryAssignTavern(unit.Handle, fieldRepo.AllBuildings);
	}
}
