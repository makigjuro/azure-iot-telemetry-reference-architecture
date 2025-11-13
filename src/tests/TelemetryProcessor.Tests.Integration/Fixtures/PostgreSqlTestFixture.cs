using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using TelemetryProcessor.Infrastructure.Database;

namespace TelemetryProcessor.Tests.Integration.Fixtures;

/// <summary>
/// Test fixture for PostgreSQL Testcontainer.
/// Provides a disposable PostgreSQL database instance for integration tests.
/// </summary>
public sealed class PostgreSqlTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer;

    public PostgreSqlTestFixture()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .WithDatabase("iot_telemetry_test")
            .WithUsername("testuser")
            .WithPassword("testpass123!")
            .WithCleanUp(true)
            .Build();
    }

    public string ConnectionString => _postgreSqlContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        // Start the PostgreSQL container
        await _postgreSqlContainer.StartAsync();

        // Apply EF Core migrations
        await using var context = CreateDbContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }

    /// <summary>
    /// Creates a new DbContext instance with the test connection string.
    /// </summary>
    public IoTTelemetryDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<IoTTelemetryDbContext>();
        optionsBuilder.UseNpgsql(ConnectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable("__efmigrations_history", "telemetry");
        });

        // Disable logging for tests
        optionsBuilder.UseLoggerFactory(LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Warning)));

        return new IoTTelemetryDbContext(optionsBuilder.Options);
    }
}
