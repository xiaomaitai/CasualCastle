// DI 组合根：构建 MS DI 容器并注册领域模块与适配器
using System.Collections.Generic;
using CasualCastle.Adapters.Godot;
using CasualCastle.Adapters.Godot.Battle;
using CasualCastle.Adapters.Godot.Flow;
using CasualCastle.Adapters.Persistence;
using CasualCastle.Application;
using CasualCastle.Domain.Shared;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.History;
using Microsoft.Extensions.DependencyInjection;

namespace CasualCastle;

public static class CompositionRoot
{
	public static ServiceProvider Build()
	{
		string configDbPath = Godot.ProjectSettings.GlobalizePath("res://assets/data/config.db");
		string savesDir = Godot.ProjectSettings.GlobalizePath("user://saves");
		GameDataLoader.Load(configDbPath);
		ApplyGameConfig(new SqliteConfigRepository(configDbPath));

		ServiceCollection services = new ServiceCollection();

		services.AddSingleton<IReadOnlyList<CardData>>(_ => GameDataLoader.ShopCatalog);
		services.AddDomainBuilding();
		services.AddDomainBattle();
		services.AddDomainHistory();

		services.AddSingleton<GameStateProvider>();
		services.AddSingleton<IGameState>(sp => sp.GetRequiredService<GameStateProvider>().Current);

		services.AddSingleton<IFieldUnitRepository, FieldUnitRepository>();
		services.AddSingleton<INavGridSource, NavGridSource>();
		services.AddSingleton<TavernPartyService>();
		services.AddSingleton<IDelayScheduler, DelaySchedulerAdapter>();
		services.AddSingleton<IBattleReportRepository, BattleReportStorage>();
		services.AddSingleton<IUnitRepository, SqliteUnitRepository>(_ => new SqliteUnitRepository(configDbPath));
		services.AddSingleton<ISkillRepository, SqliteSkillRepository>(_ => new SqliteSkillRepository(configDbPath));
		services.AddSingleton<IVisualAssetRepository, SqliteVisualAssetRepository>(_ => new SqliteVisualAssetRepository(configDbPath));
		services.AddSingleton<INecromancerBalanceConfig, SqliteNecromancerBalanceConfig>(_ => new SqliteNecromancerBalanceConfig(configDbPath));

		SqliteBuildingRepository buildingRepo = new SqliteBuildingRepository(configDbPath);
		services.AddSingleton<IBuildingRepository>(buildingRepo);
		services.AddSingleton<IBuildingVisualRepository>(buildingRepo);
		services.AddSingleton<IHistoryCoreLookup>(new BuildingCoreLookup(buildingRepo));
		services.AddSingleton<ITechTreeRepository, SqliteTechTreeRepository>(_ => new SqliteTechTreeRepository(configDbPath));
		services.AddSingleton<DamageMatrix>(_ => GameDataLoader.DamageMatrix);
		services.AddSingleton<CombineRules>(_ => GameDataLoader.CombineRules);
		services.AddSingleton<ISaveRepository, SaveStorage>(_ => new SaveStorage(savesDir));
		services.AddApplication();

		return services.BuildServiceProvider();
	}

	private static void ApplyGameConfig(SqliteConfigRepository repo)
	{
		GameParameters.InitialGold = repo.ReadInt(SqliteConfigRepository.EconomyConfig, "initial_gold", GameRules.InitialGold);
		GameParameters.RepairGoldPerHealth = repo.ReadInt(SqliteConfigRepository.EconomyConfig, "repair_gold_per_health", GameRules.RepairGoldPerHealth);
		GameParameters.CombatDurationSeconds = repo.ReadFloat(SqliteConfigRepository.BattleConfig, "combat_duration_seconds", GameRules.CombatDurationSeconds);
		GameParameters.CombineDurationSeconds = repo.ReadFloat(SqliteConfigRepository.BattleConfig, "combine_duration_seconds", GameRules.CombineDurationSeconds);
		GameParameters.CombatStartTimerHoldSeconds = repo.ReadFloat(SqliteConfigRepository.BattleConfig, "combat_start_hold_seconds", GameRules.CombatStartTimerHoldSeconds);
		GameCoordinatesAdapter.PixelsPerCell = repo.ReadInt(SqliteConfigRepository.DisplayConfig, "pixels_per_cell", GameCoordinatesAdapter.PixelsPerCellDefault);
		SfxPolicy.MaxPlaysPerSecond = repo.ReadInt(SqliteConfigRepository.SoundConfig, "sfx_max_plays_per_second", SfxPolicy.MaxPlaysPerSecondDefault);
		SfxPolicy.GroupMinIntervalMs = repo.ReadInt(SqliteConfigRepository.SoundConfig, "sfx_group_min_interval_ms", SfxPolicy.GroupMinIntervalMsDefault);
		SfxPolicy.OverloadThreshold = repo.ReadInt(SqliteConfigRepository.SoundConfig, "sfx_overload_threshold", SfxPolicy.OverloadThresholdDefault);
		SfxPolicy.ThresholdDecayPerSecond = repo.ReadInt(SqliteConfigRepository.SoundConfig, "sfx_threshold_decay_per_second", SfxPolicy.ThresholdDecayPerSecondDefault);
	}
}
