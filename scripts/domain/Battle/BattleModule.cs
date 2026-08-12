// 战斗模块 DI 注册
using Microsoft.Extensions.DependencyInjection;

namespace CasualCastle.Domain.Battle;

public static class BattleModule
{
	public static IServiceCollection AddDomainBattle(this IServiceCollection services)
	{
		services.AddSingleton<IUnitSpatialService, UnitSpatialService>();
		services.AddSingleton<IRvoService, RvoService>();
		services.AddSingleton<SkillService>();
		services.AddSingleton<StealthService>();
		services.AddSingleton<AuraService>();
		services.AddSingleton<ITacticalQueries, TacticalQueryService>();
		services.AddSingleton<CorpseService>();
		services.AddSingleton<BlackMagicService>();
		services.AddSingleton<ComfortService>();
		services.AddSingleton<KingExperienceService>();
		services.AddSingleton<KingSkillSet>();
		services.AddSingleton<TrapService>();
		services.AddSingleton<ManaService>();
		return services;
	}
}
