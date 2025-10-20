using IoTTelemetry.Shared.Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace IoTTelemetry.Shared.Infrastructure.Resilience;

/// <summary>
/// Pre-configured resilience pipelines for Azure services and external dependencies.
/// </summary>
public static class ResiliencePolicies
{
    /// <summary>
    /// Policy name for Azure Event Hubs operations.
    /// </summary>
    public const string EventHubsPolicy = "EventHubs";

    /// <summary>
    /// Policy name for Azure IoT Hub operations.
    /// </summary>
    public const string IoTHubPolicy = "IoTHub";

    /// <summary>
    /// Policy name for Azure Storage operations.
    /// </summary>
    public const string StoragePolicy = "Storage";

    /// <summary>
    /// Policy name for Azure Digital Twins operations.
    /// </summary>
    public const string DigitalTwinsPolicy = "DigitalTwins";

    /// <summary>
    /// Policy name for database operations.
    /// </summary>
    public const string DatabasePolicy = "Database";

    /// <summary>
    /// Policy name for Azure Service Bus operations.
    /// </summary>
    public const string ServiceBusPolicy = "ServiceBus";

    /// <summary>
    /// Creates a standard retry strategy for transient failures with exponential backoff.
    /// </summary>
    /// <param name="options">Resilience policy options.</param>
    /// <param name="logger">Optional logger for retry events.</param>
    /// <returns>Retry strategy options configured for transient failures.</returns>
    public static RetryStrategyOptions CreateRetryStrategy(
        ResiliencePolicyOptions options,
        ILogger? logger = null)
    {
        return new RetryStrategyOptions
        {
            MaxRetryAttempts = options.MaxRetryAttempts,
            BackoffType = DelayBackoffType.Exponential,
            Delay = TimeSpan.FromSeconds(options.RetryBaseDelaySeconds),
            MaxDelay = TimeSpan.FromSeconds(options.MaxRetryDelaySeconds),
            UseJitter = true, // Add jitter to prevent thundering herd
            ShouldHandle = new PredicateBuilder()
                .Handle<InfrastructureException>(ex => ex.IsTransient)
                .Handle<TimeoutException>(),
            OnRetry = args =>
            {
                logger?.LogWarning(
                    "Retry attempt {AttemptNumber} after {Delay}ms due to: {Exception}",
                    args.AttemptNumber,
                    args.RetryDelay.TotalMilliseconds,
                    args.Outcome.Exception?.Message);

                return ValueTask.CompletedTask;
            }
        };
    }

    /// <summary>
    /// Creates a circuit breaker strategy to prevent cascading failures.
    /// </summary>
    /// <param name="options">Resilience policy options.</param>
    /// <param name="logger">Optional logger for circuit breaker events.</param>
    /// <returns>Circuit breaker strategy options.</returns>
    public static CircuitBreakerStrategyOptions CreateCircuitBreakerStrategy(
        ResiliencePolicyOptions options,
        ILogger? logger = null)
    {
        return new CircuitBreakerStrategyOptions
        {
            FailureRatio = options.CircuitBreakerMinimumFailureRatio,
            MinimumThroughput = options.CircuitBreakerFailureThreshold,
            SamplingDuration = TimeSpan.FromSeconds(options.CircuitBreakerSamplingDurationSeconds),
            BreakDuration = TimeSpan.FromSeconds(options.CircuitBreakerBreakDurationSeconds),
            ShouldHandle = new PredicateBuilder()
                .Handle<InfrastructureException>(ex => ex.IsTransient)
                .Handle<TimeoutException>(),
            OnOpened = args =>
            {
                logger?.LogError(
                    "Circuit breaker opened after {FailureCount} failures. Break duration: {BreakDuration}s",
                    args.Outcome.Exception != null ? 1 : 0,
                    options.CircuitBreakerBreakDurationSeconds);

                return ValueTask.CompletedTask;
            },
            OnClosed = args =>
            {
                logger?.LogInformation("Circuit breaker closed. Service is healthy again.");
                return ValueTask.CompletedTask;
            },
            OnHalfOpened = args =>
            {
                logger?.LogInformation("Circuit breaker half-opened. Testing service health...");
                return ValueTask.CompletedTask;
            }
        };
    }

    /// <summary>
    /// Creates a timeout strategy for Azure service calls.
    /// </summary>
    /// <param name="options">Resilience policy options.</param>
    /// <param name="logger">Optional logger for timeout events.</param>
    /// <returns>Timeout strategy options.</returns>
    public static TimeoutStrategyOptions CreateTimeoutStrategy(
        ResiliencePolicyOptions options,
        ILogger? logger = null)
    {
        return new TimeoutStrategyOptions
        {
            Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds),
            OnTimeout = args =>
            {
                logger?.LogWarning(
                    "Operation timed out after {Timeout}s",
                    options.TimeoutSeconds);

                return ValueTask.CompletedTask;
            }
        };
    }

    /// <summary>
    /// Creates a resilience pipeline builder with standard strategies (retry + circuit breaker + timeout).
    /// </summary>
    /// <param name="options">Resilience policy options.</param>
    /// <param name="logger">Optional logger for resilience events.</param>
    /// <returns>Configured resilience pipeline builder.</returns>
    public static ResiliencePipelineBuilder CreateStandardPipeline(
        ResiliencePolicyOptions options,
        ILogger? logger = null)
    {
        return new ResiliencePipelineBuilder()
            .AddRetry(CreateRetryStrategy(options, logger))
            .AddCircuitBreaker(CreateCircuitBreakerStrategy(options, logger))
            .AddTimeout(CreateTimeoutStrategy(options, logger));
    }

    /// <summary>
    /// Creates a resilience pipeline builder with retry and timeout only (no circuit breaker).
    /// Useful for operations where circuit breaking is not desired.
    /// </summary>
    /// <param name="options">Resilience policy options.</param>
    /// <param name="logger">Optional logger for resilience events.</param>
    /// <returns>Configured resilience pipeline builder.</returns>
    public static ResiliencePipelineBuilder CreateRetryTimeoutPipeline(
        ResiliencePolicyOptions options,
        ILogger? logger = null)
    {
        return new ResiliencePipelineBuilder()
            .AddRetry(CreateRetryStrategy(options, logger))
            .AddTimeout(CreateTimeoutStrategy(options, logger));
    }
}
