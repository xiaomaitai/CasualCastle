using System.Collections.Generic;
using CasualCastle.Adapters.Godot.Battle;
using CasualCastle.Adapters.Godot.Flow;
using CasualCastle.Adapters.Persistence;
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
		GameDataLoader.Load();

		ServiceCollection services = new ServiceCollection();

		services.AddSingleton<IReadOnlyList<CardData>>(_ => GameDataLoader.ShopCatalog);
		services.AddDomainBuilding();
		services.AddDomainBattle();
		services.AddDomainHistory();

		services.AddSingleton<GameStateProvider>();
		services.AddSingleton<IGameState>(sp => sp.GetRequiredService<GameStateProvider>().Current);

		services.AddSingleton<IFieldUnitRepository, FieldUnitRepository>();
		services.AddSingleton<IBattleReportRepository, BattleReportStorage>();
		services.AddSingleton<IUnitRepository, SqliteUnitRepository>();
		services.AddSingleton<ISkillRepository, SqliteSkillRepository>();

		SqliteBuildingRepository buildingRepo = new SqliteBuildingRepository();
		services.AddSingleton<IBuildingRepository>(buildingRepo);
		services.AddSingleton<IBuildingVisualRepository>(buildingRepo);
		services.AddSingleton<ITechTreeRepository, SqliteTechTreeRepository>();
		services.AddSingleton<DamageMatrix>(_ => GameDataLoader.DamageMatrix);
		services.AddSingleton<CombineRules>(_ => GameDataLoader.CombineRules);
		services.AddSingleton<ISaveRepository, SaveStorage>();
		services.AddSingleton<IGameSessionService, GameSessionService>();

		return services.BuildServiceProvider();
	}
}
