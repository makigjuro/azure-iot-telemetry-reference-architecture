using IoTTelemetry.Shared.Infrastructure.Observability;

namespace IoTTelemetry.Shared.Infrastructure.Tests.Observability;

public class InstrumentationMetricsTests
{
    [Fact]
    public void Meter_ShouldNotBeNull()
    {
        // Act
        var meter = InstrumentationMetrics.Meter;

        // Assert
        meter.Should().NotBeNull();
        meter.Name.Should().Be(TelemetryConstants.MeterName);
    }

    [Fact]
    public void MessagesReceived_ShouldBeCounter()
    {
        // Assert
        InstrumentationMetrics.MessagesReceived.Should().NotBeNull();
        InstrumentationMetrics.MessagesReceived.Name.Should().Be(TelemetryConstants.Metrics.MessagesReceived);
    }

    [Fact]
    public void MessagesProcessed_ShouldBeCounter()
    {
        // Assert
        InstrumentationMetrics.MessagesProcessed.Should().NotBeNull();
        InstrumentationMetrics.MessagesProcessed.Name.Should().Be(TelemetryConstants.Metrics.MessagesProcessed);
    }

    [Fact]
    public void MessagesFailed_ShouldBeCounter()
    {
        // Assert
        InstrumentationMetrics.MessagesFailed.Should().NotBeNull();
        InstrumentationMetrics.MessagesFailed.Name.Should().Be(TelemetryConstants.Metrics.MessagesFailed);
    }

    [Fact]
    public void OperationDuration_ShouldBeHistogram()
    {
        // Assert
        InstrumentationMetrics.OperationDuration.Should().NotBeNull();
        InstrumentationMetrics.OperationDuration.Name.Should().Be(TelemetryConstants.Metrics.OperationDuration);
    }

    [Fact]
    public void ProcessingDuration_ShouldBeHistogram()
    {
        // Assert
        InstrumentationMetrics.ProcessingDuration.Should().NotBeNull();
        InstrumentationMetrics.ProcessingDuration.Name.Should().Be(TelemetryConstants.Metrics.ProcessingDuration);
    }

    [Fact]
    public void EventHubsMessages_ShouldBeCounter()
    {
        // Assert
        InstrumentationMetrics.EventHubsMessages.Should().NotBeNull();
        InstrumentationMetrics.EventHubsMessages.Name.Should().Be(TelemetryConstants.Metrics.EventHubsMessageCount);
    }

    [Fact]
    public void EventHubsBatchSize_ShouldBeHistogram()
    {
        // Assert
        InstrumentationMetrics.EventHubsBatchSize.Should().NotBeNull();
        InstrumentationMetrics.EventHubsBatchSize.Name.Should().Be(TelemetryConstants.Metrics.EventHubsBatchSize);
    }

    [Fact]
    public void IoTHubCommandsSent_ShouldBeCounter()
    {
        // Assert
        InstrumentationMetrics.IoTHubCommandsSent.Should().NotBeNull();
        InstrumentationMetrics.IoTHubCommandsSent.Name.Should().Be(TelemetryConstants.Metrics.IoTHubCommandsSent);
    }

    [Fact]
    public void StorageBytesWritten_ShouldBeCounter()
    {
        // Assert
        InstrumentationMetrics.StorageBytesWritten.Should().NotBeNull();
        InstrumentationMetrics.StorageBytesWritten.Name.Should().Be(TelemetryConstants.Metrics.StorageBytesWritten);
    }

    [Fact]
    public void StorageBytesRead_ShouldBeCounter()
    {
        // Assert
        InstrumentationMetrics.StorageBytesRead.Should().NotBeNull();
        InstrumentationMetrics.StorageBytesRead.Name.Should().Be(TelemetryConstants.Metrics.StorageBytesRead);
    }

    [Fact]
    public void DatabaseQueriesExecuted_ShouldBeCounter()
    {
        // Assert
        InstrumentationMetrics.DatabaseQueriesExecuted.Should().NotBeNull();
        InstrumentationMetrics.DatabaseQueriesExecuted.Name.Should().Be(TelemetryConstants.Metrics.DatabaseQueriesExecuted);
    }

    [Fact]
    public void AlertsTriggered_ShouldBeCounter()
    {
        // Assert
        InstrumentationMetrics.AlertsTriggered.Should().NotBeNull();
        InstrumentationMetrics.AlertsTriggered.Name.Should().Be(TelemetryConstants.Metrics.AlertsTriggered);
    }

    [Fact]
    public void TelemetryReadingsIngested_ShouldBeCounter()
    {
        // Assert
        InstrumentationMetrics.TelemetryReadingsIngested.Should().NotBeNull();
        InstrumentationMetrics.TelemetryReadingsIngested.Name.Should().Be(TelemetryConstants.Metrics.TelemetryReadingsIngested);
    }

    [Fact]
    public void CreateActiveDevicesGauge_ShouldReturnObservableGauge()
    {
        // Arrange
        var deviceCount = 100;

        // Act
        var gauge = InstrumentationMetrics.CreateActiveDevicesGauge(() => deviceCount);

        // Assert
        gauge.Should().NotBeNull();
    }

    [Fact]
    public void MessagesReceived_Add_ShouldNotThrow()
    {
        // Arrange
        var tags = new[]
        {
            new KeyValuePair<string, object?>(TelemetryConstants.Tags.MessagingSystem, TelemetryConstants.MessagingSystems.EventHubs),
            new KeyValuePair<string, object?>(TelemetryConstants.Tags.MessagingDestination, "telemetry-hub")
        };

        // Act
        var act = () => InstrumentationMetrics.MessagesReceived.Add(10, tags);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void OperationDuration_Record_ShouldNotThrow()
    {
        // Arrange
        var tags = new[]
        {
            new KeyValuePair<string, object?>(TelemetryConstants.Tags.MessagingSystem, TelemetryConstants.MessagingSystems.EventHubs),
            new KeyValuePair<string, object?>(TelemetryConstants.Tags.MessagingOperation, TelemetryConstants.MessagingOperations.Receive)
        };

        // Act
        var act = () => InstrumentationMetrics.OperationDuration.Record(125.5, tags);

        // Assert
        act.Should().NotThrow();
    }
}
