using Microsoft.Extensions.DependencyInjection;

namespace CasualCastle.Domain.Shared;

public static class SharedModule
{
    public static IServiceCollection AddDomainShared(this IServiceCollection services)
    {
        return services;
    }
}
