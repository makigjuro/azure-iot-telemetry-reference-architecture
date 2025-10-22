using Azure.Core;
using Azure.Identity;
using IoTTelemetry.Shared.Infrastructure.ManagedIdentity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IoTTelemetry.Shared.Infrastructure.Tests.ManagedIdentity;

public class ManagedIdentityExtensionsTests
{
    [Fact]
    public void AddAzureManagedIdentity_WithoutClientId_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAzureManagedIdentity();

        // Assert
        var provider = services.BuildServiceProvider();
        var credential = provider.GetService<TokenCredential>();
        var factory = provider.GetService<AzureClientFactory>();

        credential.Should().NotBeNull();
        credential.Should().BeOfType<DefaultAzureCredential>();
        factory.Should().NotBeNull();
    }

    [Fact]
    public void AddAzureManagedIdentity_WithClientId_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var clientId = "test-client-id";

        // Act
        services.AddAzureManagedIdentity(clientId);

        // Assert
        var provider = services.BuildServiceProvider();
        var credential = provider.GetService<TokenCredential>();
        var factory = provider.GetService<AzureClientFactory>();

        credential.Should().NotBeNull();
        credential.Should().BeOfType<DefaultAzureCredential>();
        factory.Should().NotBeNull();
    }

    [Fact]
    public void AddAzureManagedIdentityOnly_WithoutClientId_ShouldRegisterManagedIdentityCredential()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAzureManagedIdentityOnly();

        // Assert
        var provider = services.BuildServiceProvider();
        var credential = provider.GetService<TokenCredential>();

        credential.Should().NotBeNull();
        credential.Should().BeOfType<ManagedIdentityCredential>();
    }

    [Fact]
    public void AddAzureManagedIdentityOnly_WithClientId_ShouldRegisterManagedIdentityCredential()
    {
        // Arrange
        var services = new ServiceCollection();
        var clientId = "test-client-id";

        // Act
        services.AddAzureManagedIdentityOnly(clientId);

        // Assert
        var provider = services.BuildServiceProvider();
        var credential = provider.GetService<TokenCredential>();

        credential.Should().NotBeNull();
        credential.Should().BeOfType<ManagedIdentityCredential>();
    }

    [Fact]
    public void AddAzureChainedCredential_ShouldRegisterChainedTokenCredential()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAzureChainedCredential();

        // Assert
        var provider = services.BuildServiceProvider();
        var credential = provider.GetService<TokenCredential>();

        credential.Should().NotBeNull();
        credential.Should().BeOfType<ChainedTokenCredential>();
    }

    [Fact]
    public void AddAzureDevelopmentCredential_ShouldRegisterChainedTokenCredential()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAzureDevelopmentCredential();

        // Assert
        var provider = services.BuildServiceProvider();
        var credential = provider.GetService<TokenCredential>();

        credential.Should().NotBeNull();
        credential.Should().BeOfType<ChainedTokenCredential>();
    }

    [Fact]
    public void AddAzureManagedIdentityFromConfiguration_WithDefaultStrategy_ShouldRegisterDefaultCredential()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AzureManagedIdentity:Strategy"] = "Default"
            })
            .Build();

        // Act
        services.AddAzureManagedIdentityFromConfiguration(configuration);

        // Assert
        var provider = services.BuildServiceProvider();
        var credential = provider.GetService<TokenCredential>();

        credential.Should().NotBeNull();
        credential.Should().BeOfType<DefaultAzureCredential>();
    }

    [Fact]
    public void AddAzureManagedIdentityFromConfiguration_WithManagedIdentityOnlyStrategy_ShouldRegisterManagedIdentity()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AzureManagedIdentity:Strategy"] = "ManagedIdentityOnly"
            })
            .Build();

        // Act
        services.AddAzureManagedIdentityFromConfiguration(configuration);

        // Assert
        var provider = services.BuildServiceProvider();
        var credential = provider.GetService<TokenCredential>();

        credential.Should().NotBeNull();
        credential.Should().BeOfType<ManagedIdentityCredential>();
    }

    [Fact]
    public void AddAzureManagedIdentityFromConfiguration_WithChainedStrategy_ShouldRegisterChainedCredential()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AzureManagedIdentity:Strategy"] = "Chained"
            })
            .Build();

        // Act
        services.AddAzureManagedIdentityFromConfiguration(configuration);

        // Assert
        var provider = services.BuildServiceProvider();
        var credential = provider.GetService<TokenCredential>();

        credential.Should().NotBeNull();
        credential.Should().BeOfType<ChainedTokenCredential>();
    }

    [Fact]
    public void AddAzureManagedIdentityFromConfiguration_WithDevelopmentStrategy_ShouldRegisterDevelopmentCredential()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AzureManagedIdentity:Strategy"] = "Development"
            })
            .Build();

        // Act
        services.AddAzureManagedIdentityFromConfiguration(configuration);

        // Assert
        var provider = services.BuildServiceProvider();
        var credential = provider.GetService<TokenCredential>();

        credential.Should().NotBeNull();
        credential.Should().BeOfType<ChainedTokenCredential>();
    }

    [Fact]
    public void AddAzureManagedIdentityFromConfiguration_WithClientId_ShouldPassClientId()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AzureManagedIdentity:Strategy"] = "ManagedIdentityOnly",
                ["AzureManagedIdentity:ClientId"] = "test-client-id"
            })
            .Build();

        // Act
        services.AddAzureManagedIdentityFromConfiguration(configuration);

        // Assert
        var provider = services.BuildServiceProvider();
        var credential = provider.GetService<TokenCredential>();

        credential.Should().NotBeNull();
        credential.Should().BeOfType<ManagedIdentityCredential>();
    }

    [Fact]
    public void AddAzureManagedIdentityFromConfiguration_WithCustomSection_ShouldReadFromCustomSection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CustomSection:Strategy"] = "ManagedIdentityOnly"
            })
            .Build();

        // Act
        services.AddAzureManagedIdentityFromConfiguration(configuration, "CustomSection");

        // Assert
        var provider = services.BuildServiceProvider();
        var credential = provider.GetService<TokenCredential>();

        credential.Should().NotBeNull();
        credential.Should().BeOfType<ManagedIdentityCredential>();
    }

    [Fact]
    public void AddAzureManagedIdentityFromConfiguration_WithMissingConfiguration_ShouldUseDefaultStrategy()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        services.AddAzureManagedIdentityFromConfiguration(configuration);

        // Assert
        var provider = services.BuildServiceProvider();
        var credential = provider.GetService<TokenCredential>();

        credential.Should().NotBeNull();
        credential.Should().BeOfType<DefaultAzureCredential>();
    }

    [Fact]
    public void AddAzureClient_ShouldRegisterClient()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAzureManagedIdentity();

        // Act
        services.AddAzureClient<FakeAzureClient>(_ => new FakeAzureClient());

        // Assert
        var provider = services.BuildServiceProvider();
        var client = provider.GetService<FakeAzureClient>();

        client.Should().NotBeNull();
    }

    [Fact]
    public void AddAzureClientWithOptions_ShouldRegisterClient()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAzureManagedIdentity();

        // Act
        services.AddAzureClientWithOptions<FakeAzureClient, FakeClientOptions>(
            (_, _) => new FakeAzureClient(),
            options => options.MaxRetries = 5);

        // Assert
        var provider = services.BuildServiceProvider();
        var client = provider.GetService<FakeAzureClient>();

        client.Should().NotBeNull();
    }

    [Fact]
    public void AddAzureClientWithOptions_WithoutConfigurator_ShouldRegisterClient()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAzureManagedIdentity();

        // Act
        services.AddAzureClientWithOptions<FakeAzureClient, FakeClientOptions>(
            (_, _) => new FakeAzureClient());

        // Assert
        var provider = services.BuildServiceProvider();
        var client = provider.GetService<FakeAzureClient>();

        client.Should().NotBeNull();
    }

    [Fact]
    public void RegisteredServices_ShouldBeSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAzureManagedIdentity();

        // Act
        var provider = services.BuildServiceProvider();
        var credential1 = provider.GetService<TokenCredential>();
        var credential2 = provider.GetService<TokenCredential>();
        var factory1 = provider.GetService<AzureClientFactory>();
        var factory2 = provider.GetService<AzureClientFactory>();

        // Assert
        credential1.Should().BeSameAs(credential2);
        factory1.Should().BeSameAs(factory2);
    }

    // Test helper classes
    private sealed class FakeAzureClient
    {
        public bool IsInitialized { get; init; } = true;
    }

    private sealed class FakeClientOptions
    {
        public int MaxRetries { get; set; } = 3;
    }
}
