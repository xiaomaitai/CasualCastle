using Microsoft.Extensions.DependencyInjection;

namespace CasualCastle.Domain.Battle;

public static class BattleModule
{
	public static IServiceCollection AddDomainBattle(this IServiceCollection services)
	{
		services.AddSingleton<UnitSpatialService>();
		return services;
	}
}
