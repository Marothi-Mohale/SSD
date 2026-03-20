using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using SSD.Application.Abstractions;
using SSD.Application.Services;
using SSD.Infrastructure.Auth;
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

        services.AddOptions<AuthOptions>()
            .Bind(configuration.GetSection(AuthOptions.SectionName))
            .PostConfigure(options =>
            {
                options.JwtIssuer = configuration["SSD_JWT_ISSUER"] ?? options.JwtIssuer;
                options.JwtAudience = configuration["SSD_JWT_AUDIENCE"] ?? options.JwtAudience;
                options.JwtSigningKey = configuration["SSD_JWT_SIGNING_KEY"] ?? options.JwtSigningKey;
            })
            .Validate(options => !string.IsNullOrWhiteSpace(options.JwtSigningKey), "JWT signing key must be configured.")
            .ValidateOnStart();

        services.AddDbContext<SsdDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddScoped<IRecommendationService, RecommendationService>();
        services.AddSingleton<IRecommendationProvider, SeedRecommendationProvider>();
        return services;
    }
}
