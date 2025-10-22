using Azure.Core;
using Azure.Identity;
using IoTTelemetry.Shared.Infrastructure.ManagedIdentity;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace IoTTelemetry.Shared.Infrastructure.Tests.ManagedIdentity;

public class AzureClientFactoryTests
{
    private readonly TokenCredential _credential;
    private readonly ILogger<AzureClientFactory> _logger;
    private readonly AzureClientFactory _factory;

    public AzureClientFactoryTests()
    {
        _credential = new DefaultAzureCredential();
        _logger = Substitute.For<ILogger<AzureClientFactory>>();
        _factory = new AzureClientFactory(_credential, _logger);
    }

    [Fact]
    public void Constructor_WithNullCredential_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new AzureClientFactory(null!, _logger);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("credential");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new AzureClientFactory(_credential, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act
        var factory = new AzureClientFactory(_credential, _logger);

        // Assert
        factory.Should().NotBeNull();
        factory.Credential.Should().BeSameAs(_credential);
    }

    [Fact]
    public void Credential_ShouldReturnConfiguredCredential()
    {
        // Act
        var result = _factory.Credential;

        // Assert
        result.Should().BeSameAs(_credential);
    }

    [Fact]
    public void CreateClient_WithNullFactory_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => _factory.CreateClient<FakeAzureClient>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateClient_WithValidFactory_ShouldCreateClient()
    {
        // Arrange
        var expectedClient = new FakeAzureClient();

        // Act
        var result = _factory.CreateClient(_ => expectedClient);

        // Assert
        result.Should().BeSameAs(expectedClient);
    }

    [Fact]
    public void CreateClient_WhenFactoryThrows_ShouldRethrowException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test error");

        // Act
        var act = () => _factory.CreateClient<FakeAzureClient>(_ => throw expectedException);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Failed to create Azure client of type FakeAzureClient*")
            .WithInnerException<InvalidOperationException>()
            .WithMessage("Test error");
    }

    [Fact]
    public void CreateClientWithOptions_WithNullFactory_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => _factory.CreateClientWithOptions<FakeAzureClient, FakeClientOptions>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateClientWithOptions_WithValidFactory_ShouldCreateClient()
    {
        // Arrange
        var expectedClient = new FakeAzureClient();

        // Act
        var result = _factory.CreateClientWithOptions<FakeAzureClient, FakeClientOptions>(
            (_, _) => expectedClient);

        // Assert
        result.Should().BeSameAs(expectedClient);
    }

    [Fact]
    public void CreateClientWithOptions_WithOptionsConfigurator_ShouldApplyConfiguration()
    {
        // Arrange
        FakeClientOptions? capturedOptions = null;
        var expectedClient = new FakeAzureClient();

        // Act
        _factory.CreateClientWithOptions<FakeAzureClient, FakeClientOptions>(
            (_, options) =>
            {
                capturedOptions = options;
                return expectedClient;
            },
            options => options.MaxRetries = 10);

        // Assert
        capturedOptions.Should().NotBeNull();
        capturedOptions!.MaxRetries.Should().Be(10);
    }

    [Fact]
    public void CreateClientWithOptions_WithoutOptionsConfigurator_ShouldUseDefaultOptions()
    {
        // Arrange
        FakeClientOptions? capturedOptions = null;
        var expectedClient = new FakeAzureClient();

        // Act
        _factory.CreateClientWithOptions<FakeAzureClient, FakeClientOptions>(
            (_, options) =>
            {
                capturedOptions = options;
                return expectedClient;
            });

        // Assert
        capturedOptions.Should().NotBeNull();
        capturedOptions!.MaxRetries.Should().Be(3); // Default value
    }

    [Fact]
    public void CreateClientWithOptions_WhenFactoryThrows_ShouldRethrowException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test error");

        // Act
        var act = () => _factory.CreateClientWithOptions<FakeAzureClient, FakeClientOptions>(
            (_, _) => throw expectedException);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Failed to create Azure client of type FakeAzureClient with options FakeClientOptions*")
            .WithInnerException<InvalidOperationException>()
            .WithMessage("Test error");
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
