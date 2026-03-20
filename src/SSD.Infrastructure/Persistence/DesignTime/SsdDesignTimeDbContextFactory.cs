using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SSD.Infrastructure.Persistence.DesignTime;

public sealed class SsdDesignTimeDbContextFactory : IDesignTimeDbContextFactory<SsdDbContext>
{
    public SsdDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SSD_POSTGRES_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=ssd;Username=ssd;Password=change-me";

        var optionsBuilder = new DbContextOptionsBuilder<SsdDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new SsdDbContext(optionsBuilder.Options);
    }
}
