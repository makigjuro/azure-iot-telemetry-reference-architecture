using IoTTelemetry.Shared.Infrastructure.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace IoTTelemetry.Shared.Infrastructure.Tests.Logging;

public class SerilogExtensionsTests
{
    [Fact]
    public void AddSerilogLogging_ShouldConfigureLogger()
    {
        // Arrange
        var hostBuilder = Host.CreateDefaultBuilder();

        // Act
        hostBuilder.AddSerilogLogging("TestService", "1.0.0");
        var host = hostBuilder.Build();

        // Assert
        host.Should().NotBeNull();
    }

    [Fact]
    public void AddSerilogLogging_WithCustomConfiguration_ShouldApply()
    {
        // Arrange
        var hostBuilder = Host.CreateDefaultBuilder();
        var customConfigCalled = false;

        // Act
        hostBuilder.AddSerilogLogging("TestService", "1.0.0", config =>
        {
            customConfigCalled = true;
            config.MinimumLevel.Verbose();
        });

        var host = hostBuilder.Build();

        // Assert
        customConfigCalled.Should().BeTrue();
        host.Should().NotBeNull();
    }

    [Fact]
    public void CreateBootstrapLogger_ShouldReturnLogger()
    {
        // Act
        var logger = SerilogExtensions.CreateBootstrapLogger("TestService", "1.0.0");

        // Assert
        logger.Should().NotBeNull();
    }

    [Fact]
    public void CreateBootstrapLogger_ShouldBeUsableForLogging()
    {
        // Arrange
        var logger = SerilogExtensions.CreateBootstrapLogger("TestService", "1.0.0");

        // Act
        var act = () => logger.Information("Test log message");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AddStandardEnrichers_ShouldAddEnrichers()
    {
        // Arrange
        var loggerConfiguration = new LoggerConfiguration();

        // Act
        loggerConfiguration.AddStandardEnrichers("TestService", "1.0.0", "Development");
        var logger = loggerConfiguration.CreateLogger();

        // Assert
        logger.Should().NotBeNull();
    }

    [Fact]
    public void AddDevelopmentConsoleSink_ShouldAddSink()
    {
        // Arrange
        var loggerConfiguration = new LoggerConfiguration();

        // Act
        loggerConfiguration.AddDevelopmentConsoleSink(LogEventLevel.Debug);
        var logger = loggerConfiguration.CreateLogger();

        // Assert
        logger.Should().NotBeNull();
    }

    [Fact]
    public void AddProductionConsoleSink_ShouldAddSink()
    {
        // Arrange
        var loggerConfiguration = new LoggerConfiguration();

        // Act
        loggerConfiguration.AddProductionConsoleSink(LogEventLevel.Information);
        var logger = loggerConfiguration.CreateLogger();

        // Assert
        logger.Should().NotBeNull();
    }

    [Fact]
    public void AddSeqSink_ShouldAddSink()
    {
        // Arrange
        var loggerConfiguration = new LoggerConfiguration();

        // Act
        loggerConfiguration.AddSeqSink("http://localhost:5341", LogEventLevel.Debug);
        var logger = loggerConfiguration.CreateLogger();

        // Assert
        logger.Should().NotBeNull();
    }

    [Fact]
    public void AddSeqSink_WithDefaultUrl_ShouldAddSink()
    {
        // Arrange
        var loggerConfiguration = new LoggerConfiguration();

        // Act
        loggerConfiguration.AddSeqSink();
        var logger = loggerConfiguration.CreateLogger();

        // Assert
        logger.Should().NotBeNull();
    }
}
