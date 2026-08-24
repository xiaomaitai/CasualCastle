// DI 组合根：构建 MS DI 容器并注册领域模块与适配器
using System.Collections.Generic;
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
}
