using IoTTelemetry.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using TelemetryProcessor.Infrastructure.Database.Configurations;

namespace TelemetryProcessor.Infrastructure.Database;

/// <summary>
/// EF Core DbContext for IoT telemetry system.
/// </summary>
public sealed class IoTTelemetryDbContext : DbContext
{
    public IoTTelemetryDbContext(DbContextOptions<IoTTelemetryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Alert> Alerts => Set<Alert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfiguration(new DeviceConfiguration());
        modelBuilder.ApplyConfiguration(new AlertConfiguration());

        // Set default schema
        modelBuilder.HasDefaultSchema("telemetry");
    }
}
