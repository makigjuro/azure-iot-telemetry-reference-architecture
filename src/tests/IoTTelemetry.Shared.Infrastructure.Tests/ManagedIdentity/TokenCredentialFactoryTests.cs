using Azure.Core;
using Azure.Identity;
using IoTTelemetry.Shared.Infrastructure.ManagedIdentity;

namespace IoTTelemetry.Shared.Infrastructure.Tests.ManagedIdentity;

public class TokenCredentialFactoryTests
{
    [Fact]
    public void CreateDefault_WithoutClientId_ShouldReturnDefaultAzureCredential()
    {
        // Act
        var credential = TokenCredentialFactory.CreateDefault();

        // Assert
        credential.Should().NotBeNull();
        credential.Should().BeOfType<DefaultAzureCredential>();
    }

    [Fact]
    public void CreateDefault_WithClientId_ShouldReturnDefaultAzureCredential()
    {
        // Arrange
        var clientId = "test-client-id-123";

        // Act
        var credential = TokenCredentialFactory.CreateDefault(clientId);

        // Assert
        credential.Should().NotBeNull();
        credential.Should().BeOfType<DefaultAzureCredential>();
    }

    [Fact]
    public void CreateManagedIdentity_WithoutClientId_ShouldReturnManagedIdentityCredential()
    {
        // Act
        var credential = TokenCredentialFactory.CreateManagedIdentity();

        // Assert
        credential.Should().NotBeNull();
        credential.Should().BeOfType<ManagedIdentityCredential>();
    }

    [Fact]
    public void CreateManagedIdentity_WithClientId_ShouldReturnManagedIdentityCredential()
    {
        // Arrange
        var clientId = "test-client-id-456";

        // Act
        var credential = TokenCredentialFactory.CreateManagedIdentity(clientId);

        // Assert
        credential.Should().NotBeNull();
        credential.Should().BeOfType<ManagedIdentityCredential>();
    }

    [Fact]
    public void CreateManagedIdentity_WithEmptyClientId_ShouldReturnManagedIdentityCredential()
    {
        // Act
        var credential = TokenCredentialFactory.CreateManagedIdentity(string.Empty);

        // Assert
        credential.Should().NotBeNull();
        credential.Should().BeOfType<ManagedIdentityCredential>();
    }

    [Fact]
    public void CreateChained_WithoutClientId_ShouldReturnChainedTokenCredential()
    {
        // Act
        var credential = TokenCredentialFactory.CreateChained();

        // Assert
        credential.Should().NotBeNull();
        credential.Should().BeOfType<ChainedTokenCredential>();
    }

    [Fact]
    public void CreateChained_WithClientId_ShouldReturnChainedTokenCredential()
    {
        // Arrange
        var clientId = "test-client-id-789";

        // Act
        var credential = TokenCredentialFactory.CreateChained(clientId);

        // Assert
        credential.Should().NotBeNull();
        credential.Should().BeOfType<ChainedTokenCredential>();
    }

    [Fact]
    public void CreateForDevelopment_ShouldReturnChainedTokenCredential()
    {
        // Act
        var credential = TokenCredentialFactory.CreateForDevelopment();

        // Assert
        credential.Should().NotBeNull();
        credential.Should().BeOfType<ChainedTokenCredential>();
    }

    [Fact]
    public void CreateDefault_MultipleCalls_ShouldReturnDifferentInstances()
    {
        // Act
        var credential1 = TokenCredentialFactory.CreateDefault();
        var credential2 = TokenCredentialFactory.CreateDefault();

        // Assert
        credential1.Should().NotBeSameAs(credential2);
    }

    [Fact]
    public void CreateManagedIdentity_MultipleCalls_ShouldReturnDifferentInstances()
    {
        // Act
        var credential1 = TokenCredentialFactory.CreateManagedIdentity();
        var credential2 = TokenCredentialFactory.CreateManagedIdentity();

        // Assert
        credential1.Should().NotBeSameAs(credential2);
    }
}
