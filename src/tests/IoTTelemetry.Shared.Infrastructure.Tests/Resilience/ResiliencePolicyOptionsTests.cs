using IoTTelemetry.Shared.Infrastructure.Resilience;

namespace IoTTelemetry.Shared.Infrastructure.Tests.Resilience;

public class ResiliencePolicyOptionsTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var options = new ResiliencePolicyOptions();

        // Assert
        options.MaxRetryAttempts.Should().Be(3);
        options.RetryBaseDelaySeconds.Should().Be(2);
        options.MaxRetryDelaySeconds.Should().Be(30);
        options.TimeoutSeconds.Should().Be(30);
        options.CircuitBreakerFailureThreshold.Should().Be(5);
        options.CircuitBreakerSamplingDurationSeconds.Should().Be(30);
        options.CircuitBreakerBreakDurationSeconds.Should().Be(60);
        options.CircuitBreakerMinimumFailureRatio.Should().Be(0.5);
        options.BulkheadMaxParallelism.Should().Be(10);
        options.BulkheadQueueLimit.Should().Be(20);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        // Arrange
        var options = new ResiliencePolicyOptions();

        // Act
        options.MaxRetryAttempts = 5;
        options.RetryBaseDelaySeconds = 1;
        options.MaxRetryDelaySeconds = 60;
        options.TimeoutSeconds = 15;
        options.CircuitBreakerFailureThreshold = 10;
        options.CircuitBreakerSamplingDurationSeconds = 60;
        options.CircuitBreakerBreakDurationSeconds = 120;
        options.CircuitBreakerMinimumFailureRatio = 0.7;
        options.BulkheadMaxParallelism = 20;
        options.BulkheadQueueLimit = 40;

        // Assert
        options.MaxRetryAttempts.Should().Be(5);
        options.RetryBaseDelaySeconds.Should().Be(1);
        options.MaxRetryDelaySeconds.Should().Be(60);
        options.TimeoutSeconds.Should().Be(15);
        options.CircuitBreakerFailureThreshold.Should().Be(10);
        options.CircuitBreakerSamplingDurationSeconds.Should().Be(60);
        options.CircuitBreakerBreakDurationSeconds.Should().Be(120);
        options.CircuitBreakerMinimumFailureRatio.Should().Be(0.7);
        options.BulkheadMaxParallelism.Should().Be(20);
        options.BulkheadQueueLimit.Should().Be(40);
    }
}
