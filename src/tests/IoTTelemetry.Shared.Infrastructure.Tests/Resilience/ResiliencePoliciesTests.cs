using IoTTelemetry.Shared.Infrastructure.Exceptions;
using IoTTelemetry.Shared.Infrastructure.Resilience;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;
using Polly.Timeout;

namespace IoTTelemetry.Shared.Infrastructure.Tests.Resilience;

public class ResiliencePoliciesTests
{
    private readonly ResiliencePolicyOptions _options = new();

    [Fact]
    public void CreateRetryStrategy_ShouldConfigureExponentialBackoff()
    {
        // Act
        var strategy = ResiliencePolicies.CreateRetryStrategy(_options);

        // Assert
        strategy.Should().NotBeNull();
        strategy.MaxRetryAttempts.Should().Be(3);
        strategy.BackoffType.Should().Be(DelayBackoffType.Exponential);
        strategy.Delay.Should().Be(TimeSpan.FromSeconds(2));
        strategy.MaxDelay.Should().Be(TimeSpan.FromSeconds(30));
        strategy.UseJitter.Should().BeTrue();
    }

    [Fact]
    public void CreateRetryStrategy_WithCustomOptions_ShouldApplyConfiguration()
    {
        // Arrange
        var customOptions = new ResiliencePolicyOptions
        {
            MaxRetryAttempts = 5,
            RetryBaseDelaySeconds = 1,
            MaxRetryDelaySeconds = 60
        };

        // Act
        var strategy = ResiliencePolicies.CreateRetryStrategy(customOptions);

        // Assert
        strategy.MaxRetryAttempts.Should().Be(5);
        strategy.Delay.Should().Be(TimeSpan.FromSeconds(1));
        strategy.MaxDelay.Should().Be(TimeSpan.FromSeconds(60));
    }

    [Fact]
    public void CreateCircuitBreakerStrategy_ShouldConfigureThresholds()
    {
        // Act
        var strategy = ResiliencePolicies.CreateCircuitBreakerStrategy(_options);

        // Assert
        strategy.Should().NotBeNull();
        strategy.FailureRatio.Should().Be(0.5);
        strategy.MinimumThroughput.Should().Be(5);
        strategy.SamplingDuration.Should().Be(TimeSpan.FromSeconds(30));
        strategy.BreakDuration.Should().Be(TimeSpan.FromSeconds(60));
    }

    [Fact]
    public void CreateCircuitBreakerStrategy_WithCustomOptions_ShouldApplyConfiguration()
    {
        // Arrange
        var customOptions = new ResiliencePolicyOptions
        {
            CircuitBreakerFailureThreshold = 10,
            CircuitBreakerSamplingDurationSeconds = 60,
            CircuitBreakerBreakDurationSeconds = 120,
            CircuitBreakerMinimumFailureRatio = 0.7
        };

        // Act
        var strategy = ResiliencePolicies.CreateCircuitBreakerStrategy(customOptions);

        // Assert
        strategy.FailureRatio.Should().Be(0.7);
        strategy.MinimumThroughput.Should().Be(10);
        strategy.SamplingDuration.Should().Be(TimeSpan.FromSeconds(60));
        strategy.BreakDuration.Should().Be(TimeSpan.FromSeconds(120));
    }

    [Fact]
    public void CreateTimeoutStrategy_ShouldConfigureTimeout()
    {
        // Act
        var strategy = ResiliencePolicies.CreateTimeoutStrategy(_options);

        // Assert
        strategy.Should().NotBeNull();
        strategy.Timeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void CreateTimeoutStrategy_WithCustomOptions_ShouldApplyConfiguration()
    {
        // Arrange
        var customOptions = new ResiliencePolicyOptions
        {
            TimeoutSeconds = 15
        };

        // Act
        var strategy = ResiliencePolicies.CreateTimeoutStrategy(customOptions);

        // Assert
        strategy.Timeout.Should().Be(TimeSpan.FromSeconds(15));
    }

    [Fact]
    public async Task RetryStrategy_WithTransientException_ShouldRetry()
    {
        // Arrange
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(ResiliencePolicies.CreateRetryStrategy(_options, NullLogger.Instance))
            .Build();

        var attemptCount = 0;
        Func<ValueTask<string>> operation = async () =>
        {
            attemptCount++;
            if (attemptCount < 3)
            {
                throw InfrastructureException.AzureService("TestService", "Transient failure", isTransient: true);
            }
            await Task.CompletedTask;
            return "Success";
        };

        // Act
        var result = await pipeline.ExecuteAsync(async _ => await operation());

        // Assert
        result.Should().Be("Success");
        attemptCount.Should().Be(3); // Initial attempt + 2 retries
    }

    [Fact]
    public async Task RetryStrategy_WithNonTransientException_ShouldNotRetry()
    {
        // Arrange
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(ResiliencePolicies.CreateRetryStrategy(_options, NullLogger.Instance))
            .Build();

        var attemptCount = 0;
        Func<ValueTask<string>> operation = async () =>
        {
            attemptCount++;
            throw InfrastructureException.AzureService("TestService", "Non-transient failure", isTransient: false);
#pragma warning disable CS0162 // Unreachable code detected
            await Task.CompletedTask;
            return "Success";
#pragma warning restore CS0162
        };

        // Act
        var act = async () => await pipeline.ExecuteAsync(async _ => await operation());

        // Assert
        await act.Should().ThrowAsync<InfrastructureException>();
        attemptCount.Should().Be(1); // Only initial attempt, no retries
    }

    [Fact]
    public async Task TimeoutStrategy_WithLongRunningOperation_ShouldTimeout()
    {
        // Arrange
        var customOptions = new ResiliencePolicyOptions { TimeoutSeconds = 1 };
        var pipeline = new ResiliencePipelineBuilder()
            .AddTimeout(ResiliencePolicies.CreateTimeoutStrategy(customOptions, NullLogger.Instance))
            .Build();

        Func<CancellationToken, ValueTask<string>> operation = async (ct) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
            return "Success";
        };

        // Act
        var act = async () => await pipeline.ExecuteAsync(operation);

        // Assert
        await act.Should().ThrowAsync<TimeoutRejectedException>();
    }

    [Fact]
    public async Task TimeoutStrategy_WithQuickOperation_ShouldComplete()
    {
        // Arrange
        var pipeline = new ResiliencePipelineBuilder()
            .AddTimeout(ResiliencePolicies.CreateTimeoutStrategy(_options, NullLogger.Instance))
            .Build();

        Func<CancellationToken, ValueTask<string>> operation = async (ct) =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100), ct);
            return "Success";
        };

        // Act
        var result = await pipeline.ExecuteAsync(operation);

        // Assert
        result.Should().Be("Success");
    }

    [Fact]
    public void CreateStandardPipeline_ShouldIncludeAllStrategies()
    {
        // Act
        var builder = ResiliencePolicies.CreateStandardPipeline(_options, NullLogger.Instance);

        // Assert
        builder.Should().NotBeNull();
        var pipeline = builder.Build();
        pipeline.Should().NotBeNull();
    }

    [Fact]
    public void CreateRetryTimeoutPipeline_ShouldIncludeRetryAndTimeout()
    {
        // Act
        var builder = ResiliencePolicies.CreateRetryTimeoutPipeline(_options, NullLogger.Instance);

        // Assert
        builder.Should().NotBeNull();
        var pipeline = builder.Build();
        pipeline.Should().NotBeNull();
    }

    [Fact]
    public async Task StandardPipeline_WithTransientFailure_ShouldRetryAndSucceed()
    {
        // Arrange
        var pipeline = ResiliencePolicies.CreateStandardPipeline(_options, NullLogger.Instance).Build();

        var attemptCount = 0;
        Func<ValueTask<string>> operation = async () =>
        {
            attemptCount++;
            if (attemptCount < 2)
            {
                throw InfrastructureException.AzureService("EventHubs", "Transient failure", isTransient: true);
            }
            await Task.CompletedTask;
            return "Success";
        };

        // Act
        var result = await pipeline.ExecuteAsync(async _ => await operation());

        // Assert
        result.Should().Be("Success");
        attemptCount.Should().Be(2); // Initial + 1 retry
    }
}
