using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using SSD.Application.Abstractions;
using SSD.Application.Services;
using SSD.Infrastructure.Auth;
using SSD.Infrastructure.Recommendations;
using SSD.Infrastructure.Persistence;
using SSD.Infrastructure.Spotify;

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

        services.AddOptions<SpotifyOptions>()
            .Bind(configuration.GetSection(SpotifyOptions.SectionName))
            .PostConfigure(options =>
            {
                options.ClientId = configuration["SSD_SPOTIFY_CLIENT_ID"] ?? options.ClientId;
                options.ClientSecret = configuration["SSD_SPOTIFY_CLIENT_SECRET"] ?? options.ClientSecret;
                options.RedirectUri = configuration["SSD_SPOTIFY_REDIRECT_URI"] ?? options.RedirectUri;
                options.Scopes = configuration["SSD_SPOTIFY_SCOPES"] ?? options.Scopes;
            })
            .Validate(options => !string.IsNullOrWhiteSpace(options.ClientId), "Spotify client id must be configured.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ClientSecret), "Spotify client secret must be configured.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.RedirectUri), "Spotify redirect uri must be configured.")
            .ValidateOnStart();

        services.AddDbContext<SsdDbContext>(options => options.UseNpgsql(connectionString));
        services.AddDataProtection();
        services.AddTransient<SpotifyRetryHandler>();
        services.AddHttpClient<ISpotifyApiClient, SpotifyApiClient>().AddHttpMessageHandler<SpotifyRetryHandler>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISpotifyService, SpotifyService>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<SpotifyOAuthStateProtector>();
        services.AddSingleton<ISpotifyTokenProtector, SpotifyTokenProtector>();
        services.AddSingleton<IMoodRuleCatalog, InitialMoodRuleCatalog>();
        services.AddSingleton<IMoodRuleScorer, MoodRuleScorer>();
        services.AddScoped<IRecommendationService, RecommendationService>();
        services.AddSingleton<IRecommendationProvider, SeedRecommendationProvider>();
        return services;
    }
}
