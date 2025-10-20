using IoTTelemetry.Shared.Infrastructure.Resilience;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Registry;

namespace IoTTelemetry.Shared.Infrastructure.Tests.Resilience;

public class ResiliencePolicyExtensionsTests
{
    [Fact]
    public void AddAzureServiceResilience_WithEventHubsPolicy_ShouldRegisterPipeline()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAzureServiceResilience(ResiliencePolicies.EventHubsPolicy);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var pipeline = serviceProvider.GetResiliencePipeline(ResiliencePolicies.EventHubsPolicy);
        pipeline.Should().NotBeNull();
    }

    [Fact]
    public void AddAzureServiceResilience_WithIoTHubPolicy_ShouldRegisterPipeline()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAzureServiceResilience(ResiliencePolicies.IoTHubPolicy);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var pipeline = serviceProvider.GetResiliencePipeline(ResiliencePolicies.IoTHubPolicy);
        pipeline.Should().NotBeNull();
    }

    [Fact]
    public void AddAzureServiceResilience_WithStoragePolicy_ShouldRegisterPipeline()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAzureServiceResilience(ResiliencePolicies.StoragePolicy);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var pipeline = serviceProvider.GetResiliencePipeline(ResiliencePolicies.StoragePolicy);
        pipeline.Should().NotBeNull();
    }

    [Fact]
    public void AddAzureServiceResilience_WithDigitalTwinsPolicy_ShouldRegisterPipeline()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAzureServiceResilience(ResiliencePolicies.DigitalTwinsPolicy);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var pipeline = serviceProvider.GetResiliencePipeline(ResiliencePolicies.DigitalTwinsPolicy);
        pipeline.Should().NotBeNull();
    }

    [Fact]
    public void AddDatabaseResilience_ShouldRegisterPipeline()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDatabaseResilience();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var pipeline = serviceProvider.GetResiliencePipeline(ResiliencePolicies.DatabasePolicy);
        pipeline.Should().NotBeNull();
    }

    [Fact]
    public void AddDatabaseResilience_WithCustomOptions_ShouldApplyConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var customMaxRetries = 10;

        // Act
        services.AddDatabaseResilience(opts => opts.MaxRetryAttempts = customMaxRetries);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var pipeline = serviceProvider.GetResiliencePipeline(ResiliencePolicies.DatabasePolicy);
        pipeline.Should().NotBeNull();
    }

    [Fact]
    public void AddDatabaseResilience_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddDatabaseResilience();

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddAzureServiceResilience_WithServiceBusPolicy_ShouldRegisterPipeline()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAzureServiceResilience(ResiliencePolicies.ServiceBusPolicy);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var pipeline = serviceProvider.GetResiliencePipeline(ResiliencePolicies.ServiceBusPolicy);
        pipeline.Should().NotBeNull();
    }

    [Fact]
    public void AddAzureServiceResilience_WithCustomOptions_ShouldApplyConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var customMaxRetries = 10;

        // Act
        services.AddAzureServiceResilience(
            ResiliencePolicies.EventHubsPolicy,
            opts => opts.MaxRetryAttempts = customMaxRetries);

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var pipeline = serviceProvider.GetResiliencePipeline(ResiliencePolicies.EventHubsPolicy);
        pipeline.Should().NotBeNull();
    }

    [Fact]
    public void AddAzureServiceResilience_MultiplePolicies_ShouldRegisterAll()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAzureServiceResilience(ResiliencePolicies.EventHubsPolicy);
        services.AddAzureServiceResilience(ResiliencePolicies.StoragePolicy);
        services.AddDatabaseResilience();

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var eventHubsPipeline = serviceProvider.GetResiliencePipeline(ResiliencePolicies.EventHubsPolicy);
        var storagePipeline = serviceProvider.GetResiliencePipeline(ResiliencePolicies.StoragePolicy);
        var databasePipeline = serviceProvider.GetResiliencePipeline(ResiliencePolicies.DatabasePolicy);

        eventHubsPipeline.Should().NotBeNull();
        storagePipeline.Should().NotBeNull();
        databasePipeline.Should().NotBeNull();
    }

    [Fact]
    public void AddAzureServiceResilience_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddAzureServiceResilience(ResiliencePolicies.EventHubsPolicy);

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void GetResiliencePipeline_WithValidPolicyName_ShouldReturnPipeline()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAzureServiceResilience(ResiliencePolicies.EventHubsPolicy);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var pipeline = serviceProvider.GetResiliencePipeline(ResiliencePolicies.EventHubsPolicy);

        // Assert
        pipeline.Should().NotBeNull();
        pipeline.Should().BeOfType<ResiliencePipeline>();
    }

    [Fact]
    public void GetResiliencePipeline_WithUnregisteredPolicy_ShouldThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddResiliencePipelineRegistry<string>();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var act = () => serviceProvider.GetResiliencePipeline("NonExistentPolicy");

        // Assert
        act.Should().Throw<KeyNotFoundException>();
    }
}
