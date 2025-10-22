using System.Diagnostics;
using IoTTelemetry.Shared.Infrastructure.Logging;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;

namespace IoTTelemetry.Shared.Infrastructure.Tests.Logging;

public class CorrelationIdEnricherTests
{
    [Fact]
    public void Enrich_WithNoActivity_ShouldNotAddProperties()
    {
        // Arrange
        var enricher = new CorrelationIdEnricher();
        var logEvent = CreateLogEvent();

        // Act
        enricher.Enrich(logEvent, new LogEventPropertyFactory());

        // Assert
        logEvent.Properties.Should().NotContainKey("CorrelationId");
        logEvent.Properties.Should().NotContainKey("TraceId");
        logEvent.Properties.Should().NotContainKey("SpanId");
    }

    [Fact]
    public void Enrich_WithActivity_ShouldAddTraceId()
    {
        // Arrange
        var enricher = new CorrelationIdEnricher();
        var logEvent = CreateLogEvent();

        using var activity = new Activity("test-operation");
        activity.Start();

        // Act
        enricher.Enrich(logEvent, new LogEventPropertyFactory());

        // Assert
        logEvent.Properties.Should().ContainKey("TraceId");
        logEvent.Properties.Should().ContainKey("SpanId");
        logEvent.Properties.Should().ContainKey("CorrelationId");
    }

    [Fact]
    public void Enrich_WithActivity_CorrelationIdShouldMatchTraceId()
    {
        // Arrange
        var enricher = new CorrelationIdEnricher();
        var logEvent = CreateLogEvent();

        using var activity = new Activity("test-operation");
        activity.Start();

        // Act
        enricher.Enrich(logEvent, new LogEventPropertyFactory());

        // Assert
        var traceId = logEvent.Properties["TraceId"];
        var correlationId = logEvent.Properties["CorrelationId"];
        traceId.Should().Be(correlationId);
    }

    [Fact]
    public void Enrich_WithBaggageCorrelationId_ShouldUseBaggageValue()
    {
        // Arrange
        var enricher = new CorrelationIdEnricher();
        var logEvent = CreateLogEvent();

        using var activity = new Activity("test-operation");
        activity.AddBaggage("correlation-id", "custom-correlation-123");
        activity.Start();

        // Act
        enricher.Enrich(logEvent, new LogEventPropertyFactory());

        // Assert
        logEvent.Properties.Should().ContainKey("CorrelationId");
        var correlationId = logEvent.Properties["CorrelationId"].ToString();
        correlationId.Should().Contain("custom-correlation-123");
    }

    private static LogEvent CreateLogEvent()
    {
        return new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            new MessageTemplate(Array.Empty<MessageTemplateToken>()),
            Array.Empty<LogEventProperty>());
    }

    private sealed class LogEventPropertyFactory : ILogEventPropertyFactory
    {
        public LogEventProperty CreateProperty(string name, object? value, bool destructureObjects = false)
        {
            return new LogEventProperty(name, new ScalarValue(value));
        }
    }
}
