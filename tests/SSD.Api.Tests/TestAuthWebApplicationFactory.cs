using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SSD.Infrastructure.Persistence;

namespace SSD.Api.Tests;

public sealed class TestAuthWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private DbConnection? _connection;

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
                ["Security:RefreshTokenDays"] = "30"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<SsdDbContext>>();
            services.RemoveAll<SsdDbContext>();

            services.AddDbContext<SsdDbContext>(options => options.UseSqlite((SqliteConnection)_connection!));
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
