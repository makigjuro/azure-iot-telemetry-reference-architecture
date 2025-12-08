using AspNetCoreRateLimit;
using EventSubscriber.Application.Ports;
using EventSubscriber.Infrastructure.Database;
using EventSubscriber.Infrastructure.DigitalTwins;
using EventSubscriber.Infrastructure.EventGrid;
using IoTTelemetry.Shared.Infrastructure.Observability;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TelemetryProcessor.Infrastructure.Database;
using Wolverine;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .AddEnvironmentVariables()
        .Build())
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

    // Add Wolverine
    builder.Host.UseWolverine(opts =>
    {
        opts.Policies.AutoApplyTransactions();
        opts.Discovery.IncludeAssembly(typeof(EventSubscriber.Application.Commands.DeviceCreatedCommand).Assembly);
    });

    // Configure Options
    builder.Services.Configure<DigitalTwinOptions>(
        builder.Configuration.GetSection(DigitalTwinOptions.SectionName));

    // Register EF Core DbContext (shared with TelemetryProcessor)
    builder.Services.AddDbContext<IoTTelemetryDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");
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
    builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
    builder.Services.AddScoped<IDigitalTwinService, DigitalTwinService>();
    builder.Services.AddSingleton<EventGridValidator>();

    // Add OpenTelemetry
    builder.Services.AddOpenTelemetry("EventSubscriber", "1.0.0");
    builder.Services.AddEnvironmentExporters(builder.Configuration);

    // Add Health Checks
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

    // Add Rate Limiting
    builder.Services.AddMemoryCache();
    builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
    builder.Services.AddInMemoryRateLimiting();
    builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

    // Add Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "EventSubscriber API",
            Version = "v1",
            Description = "Event Grid webhook endpoint for IoT device lifecycle events"
        });
    });

    var app = builder.Build();

    // Configure middleware pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging();
    app.UseIpRateLimiting();

    // Map endpoints
    app.MapHealthChecks("/health");

    // Event Grid webhook endpoint
    app.MapPost("/api/events/devices", async (
        HttpContext context,
        EventGridValidator validator,
        IMessageBus messageBus,
        ILogger<Program> logger) =>
    {
        using var reader = new StreamReader(context.Request.Body);
        var requestBody = await reader.ReadToEndAsync();

        logger.LogDebug("Received Event Grid webhook request");

        // Check for subscription validation
        var validationCode = validator.ValidateSubscription(requestBody);
        if (validationCode is not null)
        {
            return Results.Ok(new { validationResponse = validationCode });
        }

        // Parse and process events
        var events = validator.ParseEvents(requestBody);

        foreach (var eventGridEvent in events)
        {
            logger.LogInformation(
                "Processing Event Grid event: {EventType} for subject {Subject}",
                eventGridEvent.EventType,
                eventGridEvent.Subject);

            // Route based on event type
            if (eventGridEvent.EventType.Contains("DeviceCreated", StringComparison.OrdinalIgnoreCase) ||
                eventGridEvent.EventType.Contains("DeviceConnected", StringComparison.OrdinalIgnoreCase))
            {
                // Extract device ID from subject (format: devices/{deviceId})
                var deviceId = eventGridEvent.Subject.Split('/').LastOrDefault() ?? string.Empty;

                var command = new EventSubscriber.Application.Commands.DeviceCreatedCommand(
                    deviceId,
                    eventGridEvent.EventType,
                    eventGridEvent.EventTime,
                    eventGridEvent.Data);

                await messageBus.InvokeAsync(command);
            }
            else if (eventGridEvent.EventType.Contains("DeviceDeleted", StringComparison.OrdinalIgnoreCase) ||
                     eventGridEvent.EventType.Contains("DeviceDisconnected", StringComparison.OrdinalIgnoreCase))
            {
                var deviceId = eventGridEvent.Subject.Split('/').LastOrDefault() ?? string.Empty;

                var command = new EventSubscriber.Application.Commands.DeviceDeletedCommand(
                    deviceId,
                    eventGridEvent.EventType,
                    eventGridEvent.EventTime);

                await messageBus.InvokeAsync(command);
            }
            else
            {
                logger.LogWarning(
                    "Unknown Event Grid event type: {EventType}",
                    eventGridEvent.EventType);
            }
        }

        return Results.Ok();
    })
    .WithName("DeviceLifecycleWebhook")
    .WithOpenApi()
    .WithTags("Event Grid Webhooks");

    Log.Information("EventSubscriber API starting...");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "EventSubscriber API terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

return 0;
