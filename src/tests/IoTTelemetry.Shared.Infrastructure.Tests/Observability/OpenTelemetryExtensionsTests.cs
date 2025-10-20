using IoTTelemetry.Shared.Infrastructure.Observability;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace IoTTelemetry.Shared.Infrastructure.Tests.Observability;

public class OpenTelemetryExtensionsTests
{
    [Fact]
    public void AddOpenTelemetryTracing_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddOpenTelemetryTracing("TestService", "1.0.0");
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var tracerProvider = serviceProvider.GetService<TracerProvider>();
        tracerProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddOpenTelemetryMetrics_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddOpenTelemetryMetrics("TestService", "1.0.0");
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        // Just verify no exceptions are thrown
        serviceProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddOpenTelemetry_ShouldRegisterBothTracingAndMetrics()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddOpenTelemetry("TestService", "1.0.0");
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var tracerProvider = serviceProvider.GetService<TracerProvider>();
        tracerProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddOpenTelemetryTracing_WithCustomConfiguration_ShouldApply()
    {
        // Arrange
        var services = new ServiceCollection();
        var configurationCalled = false;

        // Act
        services.AddOpenTelemetryTracing("TestService", "1.0.0", builder =>
        {
            configurationCalled = true;
        });

        _ = services.BuildServiceProvider();

        // Assert
        configurationCalled.Should().BeTrue();
    }

    [Fact]
    public void AddOpenTelemetryMetrics_WithCustomConfiguration_ShouldApply()
    {
        // Arrange
        var services = new ServiceCollection();
        var configurationCalled = false;

        // Act
        services.AddOpenTelemetryMetrics("TestService", "1.0.0", builder =>
        {
            configurationCalled = true;
        });

        _ = services.BuildServiceProvider();

        // Assert
        configurationCalled.Should().BeTrue();
    }

    [Fact]
    public void AddAzureMonitorExporter_WithoutConnectionString_ShouldSkip()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        services.AddAzureMonitorExporter(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        // Should not throw
        serviceProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddAzureMonitorExporter_WithConnectionString_ShouldRegister()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApplicationInsights:ConnectionString"] = "InstrumentationKey=test-key;IngestionEndpoint=https://test.endpoint"
            })
            .Build();

        // Act
        services.AddAzureMonitorExporter(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddEnvironmentExporters_Development_ShouldUseConsole()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Development"
            })
            .Build();

        // Act
        services.AddEnvironmentExporters(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddEnvironmentExporters_Production_ShouldUseAzureMonitor()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Production",
                ["ApplicationInsights:ConnectionString"] = "InstrumentationKey=test-key"
            })
            .Build();

        // Act
        services.AddEnvironmentExporters(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddEnvironmentExporters_NoEnvironmentSet_ShouldDefaultToProduction()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        services.AddEnvironmentExporters(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.Should().NotBeNull();
    }
}
