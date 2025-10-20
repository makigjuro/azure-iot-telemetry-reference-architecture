using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;

namespace IoTTelemetry.Shared.Infrastructure.Resilience;

/// <summary>
/// Extension methods for registering resilience policies in the dependency injection container.
/// Follows opt-in model - each service registers only the resilience policies it needs.
/// </summary>
public static class ResiliencePolicyExtensions
{
    /// <summary>
    /// Adds a resilience pipeline for Azure services with full resilience stack (retry + circuit breaker + timeout).
    /// Use policy names from <see cref="ResiliencePolicies"/> constants.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="policyName">The policy name (use constants from <see cref="ResiliencePolicies"/>).</param>
    /// <param name="configure">Optional configuration action for policy options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// // TelemetryProcessor.Infrastructure
    /// services.AddAzureServiceResilience(ResiliencePolicies.EventHubsPolicy);
    /// services.AddAzureServiceResilience(ResiliencePolicies.StoragePolicy, opts => opts.MaxRetryAttempts = 5);
    ///
    /// // AlertHandler.Infrastructure
    /// services.AddAzureServiceResilience(ResiliencePolicies.ServiceBusPolicy);
    /// services.AddAzureServiceResilience(ResiliencePolicies.IoTHubPolicy);
    /// </example>
    public static IServiceCollection AddAzureServiceResilience(
        this IServiceCollection services,
        string policyName,
        Action<ResiliencePolicyOptions>? configure = null)
    {
        var options = new ResiliencePolicyOptions();
        configure?.Invoke(options);

        services.AddResiliencePipeline(policyName, (builder, context) =>
        {
            var logger = context.ServiceProvider.GetService<ILogger<ResiliencePipeline>>();
            builder
                .AddRetry(ResiliencePolicies.CreateRetryStrategy(options, logger))
                .AddCircuitBreaker(ResiliencePolicies.CreateCircuitBreakerStrategy(options, logger))
                .AddTimeout(ResiliencePolicies.CreateTimeoutStrategy(options, logger));
        });

        return services;
    }

    /// <summary>
    /// Adds a resilience pipeline for database operations with retry and timeout only (no circuit breaker).
    /// Circuit breaker intentionally omitted to avoid prolonged service degradation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action for policy options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// // TelemetryProcessor.Infrastructure
    /// services.AddDatabaseResilience();
    /// services.AddDatabaseResilience(opts => opts.MaxRetryAttempts = 5);
    /// </example>
    public static IServiceCollection AddDatabaseResilience(
        this IServiceCollection services,
        Action<ResiliencePolicyOptions>? configure = null)
    {
        var options = new ResiliencePolicyOptions();
        configure?.Invoke(options);

        services.AddResiliencePipeline(ResiliencePolicies.DatabasePolicy, (builder, context) =>
        {
            var logger = context.ServiceProvider.GetService<ILogger<ResiliencePipeline>>();
            builder
                .AddRetry(ResiliencePolicies.CreateRetryStrategy(options, logger))
                .AddTimeout(ResiliencePolicies.CreateTimeoutStrategy(options, logger));
        });

        return services;
    }

    /// <summary>
    /// Gets a named resilience pipeline from the service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="policyName">The name of the pipeline to retrieve.</param>
    /// <returns>The resilience pipeline.</returns>
    public static ResiliencePipeline GetResiliencePipeline(
        this IServiceProvider serviceProvider,
        string policyName)
    {
        var provider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();
        return provider.GetPipeline(policyName);
    }
}
