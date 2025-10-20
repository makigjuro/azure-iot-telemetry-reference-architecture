using System.Diagnostics;
using IoTTelemetry.Shared.Infrastructure.Observability;

namespace IoTTelemetry.Shared.Infrastructure.Tests.Observability;

public class ActivitySourceProviderTests
{
    [Fact]
    public void ActivitySource_ShouldNotBeNull()
    {
        // Act
        var activitySource = ActivitySourceProvider.ActivitySource;

        // Assert
        activitySource.Should().NotBeNull();
        activitySource.Name.Should().Be(TelemetryConstants.ActivitySourceName);
    }

    [Fact]
    public void StartActivity_WithName_ShouldReturnNullWhenNoListeners()
    {
        // Act
        var activity = ActivitySourceProvider.StartActivity(TelemetryConstants.Activities.EventHubsReceive);

        // Assert
        // No listeners registered, so activity should be null
        activity.Should().BeNull();
    }

    [Fact]
    public void StartActivity_WithKind_ShouldReturnNullWhenNoListeners()
    {
        // Act
        var activity = ActivitySourceProvider.StartActivity(
            TelemetryConstants.Activities.EventHubsReceive,
            ActivityKind.Consumer);

        // Assert
        activity.Should().BeNull();
    }

    [Fact]
    public void AddStandardTags_WithNullActivity_ShouldNotThrow()
    {
        // Act
        var act = () => ActivitySourceProvider.AddStandardTags(
            null,
            deviceId: "device-001",
            correlationId: "corr-123");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordException_WithNullActivity_ShouldNotThrow()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        // Act
        var act = () => ActivitySourceProvider.RecordException(null, exception);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void SetSuccess_WithNullActivity_ShouldNotThrow()
    {
        // Act
        var act = () => ActivitySourceProvider.SetSuccess(null);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void SetError_WithNullActivity_ShouldNotThrow()
    {
        // Act
        var act = () => ActivitySourceProvider.SetError(null, "Test error");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void StartActivity_WithTags_ShouldReturnNullWhenNoListeners()
    {
        // Arrange
        var tags = new List<KeyValuePair<string, object?>>
        {
            new(TelemetryConstants.Tags.DeviceId, "device-001"),
            new(TelemetryConstants.Tags.MessagingOperation, TelemetryConstants.MessagingOperations.Receive)
        };

        // Act
        var activity = ActivitySourceProvider.StartActivity(
            TelemetryConstants.Activities.EventHubsReceive,
            ActivityKind.Consumer,
            tags);

        // Assert
        activity.Should().BeNull();
    }

    [Fact]
    public void StartActivity_WithParentContext_ShouldReturnNullWhenNoListeners()
    {
        // Arrange
        var parentContext = new ActivityContext(
            ActivityTraceId.CreateRandom(),
            ActivitySpanId.CreateRandom(),
            ActivityTraceFlags.Recorded);

        // Act
        var activity = ActivitySourceProvider.StartActivity(
            TelemetryConstants.Activities.EventHubsReceive,
            ActivityKind.Consumer,
            parentContext);

        // Assert
        activity.Should().BeNull();
    }
}
