using Microsoft.Extensions.DependencyInjection;

namespace CasualCastle.Domain.Building;

public static class BuildingModule
{
	public static IServiceCollection AddDomainBuilding(this IServiceCollection services)
	{
		services.AddSingleton<ShopRules>();
		services.AddSingleton<AdjacencyService>();
		services.AddSingleton<ICombineUseCase, CombineService>();
		return services;
	}
}
