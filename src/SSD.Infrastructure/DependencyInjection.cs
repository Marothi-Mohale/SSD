using Microsoft.Extensions.DependencyInjection;
using SSD.Application.Abstractions;
using SSD.Application.Services;
using SSD.Infrastructure.Recommendations;

namespace SSD.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IRecommendationService, RecommendationService>();
        services.AddSingleton<IRecommendationProvider, SeedRecommendationProvider>();
        return services;
    }
}
