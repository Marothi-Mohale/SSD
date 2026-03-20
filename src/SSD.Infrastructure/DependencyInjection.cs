using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using SSD.Application.Abstractions;
using SSD.Application.Services;
using SSD.Infrastructure.Recommendations;
using SSD.Infrastructure.Persistence;

namespace SSD.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? configuration["SSD_POSTGRES_CONNECTION"]
            ?? "Host=localhost;Port=5432;Database=ssd;Username=ssd;Password=change-me";

        services.AddDbContext<SsdDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IRecommendationService, RecommendationService>();
        services.AddSingleton<IRecommendationProvider, SeedRecommendationProvider>();
        return services;
    }
}
