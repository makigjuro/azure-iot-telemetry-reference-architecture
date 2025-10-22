using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;

namespace IoTTelemetry.Shared.Infrastructure.Logging;

/// <summary>
/// Extension methods for configuring Serilog structured logging.
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    /// Adds Serilog with standard configuration (enrichers and environment-specific sinks).
    /// </summary>
    /// <param name="builder">The host builder.</param>
    /// <param name="serviceName">The service name.</param>
    /// <param name="serviceVersion">The service version.</param>
    /// <param name="configure">Optional additional Serilog configuration.</param>
    /// <returns>The host builder for chaining.</returns>
    public static IHostBuilder AddSerilogLogging(
        this IHostBuilder builder,
        string serviceName,
        string serviceVersion,
        Action<LoggerConfiguration>? configure = null)
    {
        return builder.UseSerilog((context, services, loggerConfiguration) =>
        {
            var environment = context.HostingEnvironment.EnvironmentName;

            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.With(new CorrelationIdEnricher())
                .Enrich.With(new ServiceContextEnricher(serviceName, serviceVersion, environment))
                .Enrich.WithMachineName()
                .Enrich.WithThreadId();

            // Configure sinks based on environment
            ConfigureSinks(loggerConfiguration, context.Configuration, environment);

            // Apply custom configuration if provided
            configure?.Invoke(loggerConfiguration);
        });
    }

    /// <summary>
    /// Configures Serilog sinks based on the environment.
    /// </summary>
    private static void ConfigureSinks(
        LoggerConfiguration loggerConfiguration,
        IConfiguration configuration,
        string environment)
    {
        var isDevelopment = environment.Equals("Development", StringComparison.OrdinalIgnoreCase);

        if (isDevelopment)
        {
            // Development: Human-readable console output + Seq
            loggerConfiguration
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
                    restrictedToMinimumLevel: LogEventLevel.Debug);

            // Add Seq if configured
            var seqUrl = configuration["Seq:ServerUrl"] ?? "http://localhost:5341";
            loggerConfiguration.WriteTo.Seq(seqUrl, restrictedToMinimumLevel: LogEventLevel.Debug);
        }
        else
        {
            // Production: JSON console output for container log aggregation
            loggerConfiguration
                .WriteTo.Console(
                    formatter: new CompactJsonFormatter(),
                    restrictedToMinimumLevel: LogEventLevel.Information);
        }

        // Add Application Insights if connection string is configured
        var appInsightsConnectionString = configuration["ApplicationInsights:ConnectionString"];
        if (!string.IsNullOrEmpty(appInsightsConnectionString))
        {
            // Application Insights integration via OpenTelemetry
            // Logs will be automatically exported through OpenTelemetry
            loggerConfiguration.WriteTo.Console(
                formatter: new JsonFormatter(),
                restrictedToMinimumLevel: LogEventLevel.Information);
        }
    }

    /// <summary>
    /// Creates a bootstrap logger for use during application startup (before DI is configured).
    /// </summary>
    /// <param name="serviceName">The service name.</param>
    /// <param name="serviceVersion">The service version.</param>
    /// <returns>A configured Serilog logger.</returns>
    public static ILogger CreateBootstrapLogger(string serviceName, string serviceVersion)
    {
        return new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.With(new ServiceContextEnricher(serviceName, serviceVersion))
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateBootstrapLogger();
    }

    /// <summary>
    /// Adds Serilog enrichers for correlation and service context.
    /// </summary>
    /// <param name="loggerConfiguration">The logger configuration.</param>
    /// <param name="serviceName">The service name.</param>
    /// <param name="serviceVersion">The service version.</param>
    /// <param name="environment">The deployment environment.</param>
    /// <returns>The logger configuration for chaining.</returns>
    public static LoggerConfiguration AddStandardEnrichers(
        this LoggerConfiguration loggerConfiguration,
        string serviceName,
        string serviceVersion,
        string? environment = null)
    {
        return loggerConfiguration
            .Enrich.FromLogContext()
            .Enrich.With(new CorrelationIdEnricher())
            .Enrich.With(new ServiceContextEnricher(serviceName, serviceVersion, environment))
            .Enrich.WithMachineName()
            .Enrich.WithThreadId();
    }

    /// <summary>
    /// Adds console sink with human-readable output for development.
    /// </summary>
    /// <param name="loggerConfiguration">The logger configuration.</param>
    /// <param name="minimumLevel">The minimum log level.</param>
    /// <returns>The logger configuration for chaining.</returns>
    public static LoggerConfiguration AddDevelopmentConsoleSink(
        this LoggerConfiguration loggerConfiguration,
        LogEventLevel minimumLevel = LogEventLevel.Debug)
    {
        return loggerConfiguration.WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
            restrictedToMinimumLevel: minimumLevel);
    }

    /// <summary>
    /// Adds console sink with JSON output for production.
    /// </summary>
    /// <param name="loggerConfiguration">The logger configuration.</param>
    /// <param name="minimumLevel">The minimum log level.</param>
    /// <returns>The logger configuration for chaining.</returns>
    public static LoggerConfiguration AddProductionConsoleSink(
        this LoggerConfiguration loggerConfiguration,
        LogEventLevel minimumLevel = LogEventLevel.Information)
    {
        return loggerConfiguration.WriteTo.Console(
            formatter: new CompactJsonFormatter(),
            restrictedToMinimumLevel: minimumLevel);
    }

    /// <summary>
    /// Adds Seq sink for local development.
    /// </summary>
    /// <param name="loggerConfiguration">The logger configuration.</param>
    /// <param name="seqUrl">The Seq server URL (default: http://localhost:5341).</param>
    /// <param name="minimumLevel">The minimum log level.</param>
    /// <returns>The logger configuration for chaining.</returns>
    public static LoggerConfiguration AddSeqSink(
        this LoggerConfiguration loggerConfiguration,
        string seqUrl = "http://localhost:5341",
        LogEventLevel minimumLevel = LogEventLevel.Debug)
    {
        return loggerConfiguration.WriteTo.Seq(seqUrl, restrictedToMinimumLevel: minimumLevel);
    }
}
