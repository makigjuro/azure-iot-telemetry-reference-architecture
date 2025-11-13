using IoTTelemetry.Shared.Infrastructure.Observability;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TelemetryProcessor.Application.Ports;
using TelemetryProcessor.Application.Validators;
using TelemetryProcessor.Infrastructure.Database;
using TelemetryProcessor.Infrastructure.EventHubs;
using TelemetryProcessor.Infrastructure.Storage;
using Wolverine;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .AddEnvironmentVariables()
        .Build())
    .CreateLogger();

try
{
    var builder = Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .UseWolverine(opts =>
        {
            // Enable local message processing
            opts.Policies.AutoApplyTransactions();

            // Discovery settings
            opts.Discovery.IncludeAssembly(typeof(TelemetryProcessor.Application.Commands.ProcessTelemetryCommand).Assembly);
        })
        .ConfigureServices((context, services) =>
        {
            // Configure Options
            services.Configure<EventHubConsumerOptions>(
                context.Configuration.GetSection(EventHubConsumerOptions.SectionName));
            services.Configure<DataLakeOptions>(
                context.Configuration.GetSection(DataLakeOptions.SectionName));

            // Register EF Core DbContext
            services.AddDbContext<IoTTelemetryDbContext>(options =>
            {
                var connectionString = context.Configuration.GetConnectionString("PostgreSQL");
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable("__efmigrations_history", "telemetry");
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                });
            });

            // Register Application Services (Ports)
            services.AddSingleton<ITelemetryValidator, TelemetryValidator>();

            // Register Infrastructure Services (Adapters)
            services.AddSingleton<ITelemetryStorage, DataLakeStorageService>();
            services.AddScoped<IDeviceMetadataRepository, DeviceMetadataRepository>();
            services.AddSingleton<IEventHubConsumer, EventHubConsumerService>();

            // Register Event Hub Consumer as Background Service
            services.AddHostedService<EventHubConsumerService>();

            // Add OpenTelemetry
            services.AddOpenTelemetry("TelemetryProcessor", "1.0.0");
            services.AddEnvironmentExporters(context.Configuration);

            // Add Health Checks
            services.AddHealthChecks()
                .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());
        });

    var host = builder.Build();

    Log.Information("TelemetryProcessor starting...");
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "TelemetryProcessor terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

return 0;
