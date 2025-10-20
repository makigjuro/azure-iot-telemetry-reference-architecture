using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace IoTTelemetry.Shared.Infrastructure.Observability;

/// <summary>
/// Extension methods for registering OpenTelemetry tracing and metrics.
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Adds OpenTelemetry distributed tracing to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceName">The name of the service (e.g., "TelemetryProcessor", "AlertHandler").</param>
    /// <param name="serviceVersion">The version of the service (default: "1.0.0").</param>
    /// <param name="configure">Optional additional configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOpenTelemetryTracing(
        this IServiceCollection services,
        string serviceName,
        string serviceVersion = "1.0.0",
        Action<TracerProviderBuilder>? configure = null)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName, serviceVersion: serviceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    [TelemetryConstants.Tags.ServiceName] = serviceName,
                    [TelemetryConstants.Tags.ServiceVersion] = serviceVersion
                }))
            .WithTracing(tracing =>
            {
                tracing
                    // Add our custom ActivitySource
                    .AddSource(TelemetryConstants.ActivitySourceName)
                    // Add automatic instrumentation for HttpClient
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.FilterHttpRequestMessage = _ => true;
                        options.EnrichWithHttpRequestMessage = (activity, request) =>
                        {
                            activity.SetTag("http.request.method", request.Method.Method);
                        };
                        options.EnrichWithHttpResponseMessage = (activity, response) =>
                        {
                            activity.SetTag("http.response.status_code", (int)response.StatusCode);
                        };
                    });

                // Apply custom configuration if provided
                configure?.Invoke(tracing);
            });

        return services;
    }

    /// <summary>
    /// Adds OpenTelemetry metrics collection to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceName">The name of the service.</param>
    /// <param name="serviceVersion">The version of the service (default: "1.0.0").</param>
    /// <param name="configure">Optional additional configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOpenTelemetryMetrics(
        this IServiceCollection services,
        string serviceName,
        string serviceVersion = "1.0.0",
        Action<MeterProviderBuilder>? configure = null)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName, serviceVersion: serviceVersion))
            .WithMetrics(metrics =>
            {
                metrics
                    // Add our custom Meter
                    .AddMeter(TelemetryConstants.MeterName);

                // Apply custom configuration if provided
                configure?.Invoke(metrics);
            });

        return services;
    }

    /// <summary>
    /// Adds both OpenTelemetry tracing and metrics (convenience method).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceName">The name of the service.</param>
    /// <param name="serviceVersion">The version of the service (default: "1.0.0").</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOpenTelemetry(
        this IServiceCollection services,
        string serviceName,
        string serviceVersion = "1.0.0")
    {
        services.AddOpenTelemetryTracing(serviceName, serviceVersion);
        services.AddOpenTelemetryMetrics(serviceName, serviceVersion);

        return services;
    }

    /// <summary>
    /// Adds Console exporter for OpenTelemetry (useful for local development).
    /// </summary>
    /// <param name="builder">The tracer provider builder.</param>
    /// <returns>The tracer provider builder for chaining.</returns>
    public static TracerProviderBuilder AddConsoleExporter(this TracerProviderBuilder builder)
    {
        return builder.AddConsoleExporter(options =>
        {
            options.Targets = ConsoleExporterOutputTargets.Console;
        });
    }

    /// <summary>
    /// Adds Console exporter for OpenTelemetry metrics (useful for local development).
    /// </summary>
    /// <param name="builder">The meter provider builder.</param>
    /// <returns>The meter provider builder for chaining.</returns>
    public static MeterProviderBuilder AddConsoleExporter(this MeterProviderBuilder builder)
    {
        return builder.AddConsoleExporter(options =>
        {
            options.Targets = ConsoleExporterOutputTargets.Console;
        });
    }

    /// <summary>
    /// Adds Azure Monitor (Application Insights) exporter for OpenTelemetry.
    /// Requires "ApplicationInsights:ConnectionString" in configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAzureMonitorExporter(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration["ApplicationInsights:ConnectionString"];

        if (string.IsNullOrEmpty(connectionString))
        {
            // If no connection string, skip Azure Monitor (useful for local dev)
            return services;
        }

        services.AddOpenTelemetry()
            .UseAzureMonitor(options =>
            {
                options.ConnectionString = connectionString;
            });

        return services;
    }

    /// <summary>
    /// Adds environment-specific exporters based on configuration.
    /// - Local development: Console exporter
    /// - Azure: Application Insights exporter
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEnvironmentExporters(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? configuration["DOTNET_ENVIRONMENT"] ?? "Production";

        if (environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
        {
            // Local development: use console exporter
            services.AddOpenTelemetry()
                .WithTracing(tracing => tracing.AddConsoleExporter())
                .WithMetrics(metrics => metrics.AddConsoleExporter());
        }
        else
        {
            // Production/Staging: use Azure Monitor
            services.AddAzureMonitorExporter(configuration);
        }

        return services;
    }
}
