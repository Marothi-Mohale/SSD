using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SSD.Infrastructure.Persistence;
using SSD.Infrastructure.Spotify;

namespace SSD.Api.Tests;

public sealed class TestAuthWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private DbConnection? _connection;
    internal SpotifyStubMessageHandler SpotifyHandler { get; } = new();

    public Task InitializeAsync()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        return _connection.OpenAsync();
    }

    public new Task DisposeAsync()
    {
        return _connection?.DisposeAsync().AsTask() ?? Task.CompletedTask;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = "Host=localhost;Database=ignored",
                ["Security:JwtIssuer"] = "SSD.Api.Tests",
                ["Security:JwtAudience"] = "SSD.Mobile.Tests",
                ["Security:JwtSigningKey"] = "test-signing-key-with-at-least-thirty-two-bytes",
                ["Security:AccessTokenMinutes"] = "15",
                ["Security:RefreshTokenDays"] = "30",
                ["Providers:Spotify:ClientId"] = "spotify-client-id",
                ["Providers:Spotify:ClientSecret"] = "spotify-client-secret",
                ["Providers:Spotify:RedirectUri"] = "https://localhost/api/spotify/link/callback",
                ["Providers:Spotify:Scopes"] = "user-read-email user-read-private user-top-read"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<SsdDbContext>>();
            services.RemoveAll<SsdDbContext>();
            services.RemoveAll<ISpotifyApiClient>();

            services.AddDbContext<SsdDbContext>(options => options.UseSqlite((SqliteConnection)_connection!));
            services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo("/tmp/ssd-test-keys"));
            services.AddHttpClient<ISpotifyApiClient, SpotifyApiClient>()
                .ConfigurePrimaryHttpMessageHandler(() => SpotifyHandler);
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SsdDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }
}
