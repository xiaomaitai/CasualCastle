using System.Collections.Generic;
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
        services.AddDomainShared();
        services.AddDomainBuilding();
        services.AddDomainBattle();
        services.AddDomainHistory();

        services.AddSingleton<IGameState>(_ =>
            CasualCastle.Adapters.Godot.AdapterRegistry.Resolve<IGameState>()
            ?? throw new System.InvalidOperationException("IGameState not registered"));

        services.AddSingleton<IFieldUnitRepository, FieldUnitRepository>();
        services.AddSingleton<IBattleReportRepository, BattleReportStorage>();
        services.AddSingleton<IUnitRepository, SqliteUnitRepository>();
		
        services.AddSingleton<IBuildingRepository, SqliteBuildingRepository>();

        return services.BuildServiceProvider();
    }
}
