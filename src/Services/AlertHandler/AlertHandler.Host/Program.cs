using AlertHandler.Application.Ports;
using AlertHandler.Infrastructure.Database;
using AlertHandler.Infrastructure.IoTHub;
using AlertHandler.Infrastructure.ServiceBus;
using IoTTelemetry.Shared.Infrastructure.Observability;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TelemetryProcessor.Infrastructure.Database;
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
            opts.Discovery.IncludeAssembly(typeof(AlertHandler.Application.Commands.ProcessAlertCommand).Assembly);
        })
        .ConfigureServices((context, services) =>
        {
            // Configure Options
            services.Configure<ServiceBusConsumerOptions>(
                context.Configuration.GetSection(ServiceBusConsumerOptions.SectionName));
            services.Configure<IoTHubOptions>(
                context.Configuration.GetSection(IoTHubOptions.SectionName));

            // Register EF Core DbContext (shared with TelemetryProcessor)
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

            // Register Infrastructure Services (Adapters)
            services.AddSingleton<IServiceBusConsumer, ServiceBusConsumerService>();
            services.AddSingleton<IDeviceCommandSender, DeviceCommandSender>();
            services.AddScoped<IAlertRepository, AlertRepository>();

            // Register Service Bus Consumer as Background Service
            services.AddHostedService<ServiceBusConsumerService>();

            // Add OpenTelemetry
            services.AddOpenTelemetry("AlertHandler", "1.0.0");
            services.AddEnvironmentExporters(context.Configuration);

            // Add Health Checks
            services.AddHealthChecks()
                .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());
        });

    var host = builder.Build();

    Log.Information("AlertHandler starting...");
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "AlertHandler terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

return 0;
