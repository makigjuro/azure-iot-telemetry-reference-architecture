namespace IoTTelemetry.Shared.Infrastructure.Resilience;

/// <summary>
/// Configuration options for resilience policies.
/// </summary>
public sealed class ResiliencePolicyOptions
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts for transient failures.
    /// Default: 3
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the base delay for exponential backoff (in seconds).
    /// Default: 2 seconds
    /// </summary>
    public int RetryBaseDelaySeconds { get; set; } = 2;

    /// <summary>
    /// Gets or sets the maximum delay between retry attempts (in seconds).
    /// Default: 30 seconds
    /// </summary>
    public int MaxRetryDelaySeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the timeout duration for Azure service calls (in seconds).
    /// Default: 30 seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the circuit breaker failure threshold (number of consecutive failures before opening).
    /// Default: 5
    /// </summary>
    public int CircuitBreakerFailureThreshold { get; set; } = 5;

    /// <summary>
    /// Gets or sets the circuit breaker sampling duration (in seconds).
    /// Default: 30 seconds
    /// </summary>
    public int CircuitBreakerSamplingDurationSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the circuit breaker break duration (how long to stay open, in seconds).
    /// Default: 60 seconds
    /// </summary>
    public int CircuitBreakerBreakDurationSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets the minimum failure ratio (0.0 to 1.0) to open the circuit.
    /// Default: 0.5 (50% failures)
    /// </summary>
    public double CircuitBreakerMinimumFailureRatio { get; set; } = 0.5;

    /// <summary>
    /// Gets or sets the bulkhead max parallelism (concurrent requests).
    /// Default: 10
    /// </summary>
    public int BulkheadMaxParallelism { get; set; } = 10;

    /// <summary>
    /// Gets or sets the bulkhead queue limit.
    /// Default: 20
    /// </summary>
    public int BulkheadQueueLimit { get; set; } = 20;
}
