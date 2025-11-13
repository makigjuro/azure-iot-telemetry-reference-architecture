using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TelemetryProcessor.Infrastructure.Database;

/// <summary>
/// Design-time DbContext factory for EF Core migrations.
/// This is only used by EF Core tooling, not at runtime.
/// </summary>
public sealed class IoTTelemetryDbContextFactory : IDesignTimeDbContextFactory<IoTTelemetryDbContext>
{
    public IoTTelemetryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IoTTelemetryDbContext>();

        // Design-time connection string (not used in production)
        // This is just for generating migrations - actual connection comes from config
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=iot_telemetry;Username=iotuser;Password=iotpass123!",
            npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__efmigrations_history", "telemetry");
            });

        return new IoTTelemetryDbContext(optionsBuilder.Options);
    }
}
