// 应用模块：用例与端口集合注册
using CasualCastle.Domain.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace CasualCastle.Application;

public static class ApplicationModule
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		services.AddSingleton<GameSessionPorts>();
		services.AddSingleton<IGameSessionService, GameSessionService>();
		services.AddSingleton<NightCombineUseCase>();
		services.AddSingleton<PhaseTransitionUseCase>();
		services.AddSingleton<MatchSessionUseCase>();
		return services;
	}
}
