using Microsoft.Extensions.DependencyInjection;

namespace CasualCastle.Domain.Battle;

public static class BattleModule
{
	public static IServiceCollection AddDomainBattle(this IServiceCollection services)
	{
		services.AddSingleton<ICombatUseCase, UnitSpatialService>();
		services.AddSingleton<IRvoService, RvoService>();
		services.AddSingleton<SkillService>();
		services.AddSingleton<ITacticalQueries, TacticalQueryService>();
		return services;
	}
}
