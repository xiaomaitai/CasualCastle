using Microsoft.Extensions.DependencyInjection;

namespace CasualCastle.Domain.History;

public static class HistoryModule
{
    public static IServiceCollection AddDomainHistory(this IServiceCollection services)
    {
        services.AddSingleton<BattleReportService>();
        services.AddSingleton<ReplayService>();
        return services;
    }
}
