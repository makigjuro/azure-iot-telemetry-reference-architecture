using IoTTelemetry.Shared.Infrastructure.Logging;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;

namespace IoTTelemetry.Shared.Infrastructure.Tests.Logging;

public class ServiceContextEnricherTests
{
    [Fact]
    public void Enrich_ShouldAddServiceName()
    {
        // Arrange
        var enricher = new ServiceContextEnricher("TestService", "1.0.0");
        var logEvent = CreateLogEvent();

        // Act
        enricher.Enrich(logEvent, new LogEventPropertyFactory());

        // Assert
        logEvent.Properties.Should().ContainKey("ServiceName");
        logEvent.Properties["ServiceName"].ToString().Should().Contain("TestService");
    }

    [Fact]
    public void Enrich_ShouldAddServiceVersion()
    {
        // Arrange
        var enricher = new ServiceContextEnricher("TestService", "1.0.0");
        var logEvent = CreateLogEvent();

        // Act
        enricher.Enrich(logEvent, new LogEventPropertyFactory());

        // Assert
        logEvent.Properties.Should().ContainKey("ServiceVersion");
        logEvent.Properties["ServiceVersion"].ToString().Should().Contain("1.0.0");
    }

    [Fact]
    public void Enrich_WithEnvironment_ShouldAddEnvironment()
    {
        // Arrange
        var enricher = new ServiceContextEnricher("TestService", "1.0.0", "Development");
        var logEvent = CreateLogEvent();

        // Act
        enricher.Enrich(logEvent, new LogEventPropertyFactory());

        // Assert
        logEvent.Properties.Should().ContainKey("Environment");
        logEvent.Properties["Environment"].ToString().Should().Contain("Development");
    }

    [Fact]
    public void Enrich_WithoutEnvironment_ShouldNotAddEnvironment()
    {
        // Arrange
        var enricher = new ServiceContextEnricher("TestService", "1.0.0");
        var logEvent = CreateLogEvent();

        // Act
        enricher.Enrich(logEvent, new LogEventPropertyFactory());

        // Assert
        logEvent.Properties.Should().NotContainKey("Environment");
    }

    [Fact]
    public void Enrich_WithNullEnvironment_ShouldNotAddEnvironment()
    {
        // Arrange
        var enricher = new ServiceContextEnricher("TestService", "1.0.0", null);
        var logEvent = CreateLogEvent();

        // Act
        enricher.Enrich(logEvent, new LogEventPropertyFactory());

        // Assert
        logEvent.Properties.Should().NotContainKey("Environment");
    }

    [Fact]
    public void Enrich_WithEmptyEnvironment_ShouldNotAddEnvironment()
    {
        // Arrange
        var enricher = new ServiceContextEnricher("TestService", "1.0.0", string.Empty);
        var logEvent = CreateLogEvent();

        // Act
        enricher.Enrich(logEvent, new LogEventPropertyFactory());

        // Assert
        logEvent.Properties.Should().NotContainKey("Environment");
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
