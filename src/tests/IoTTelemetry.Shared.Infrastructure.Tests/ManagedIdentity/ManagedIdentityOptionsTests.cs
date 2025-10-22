using IoTTelemetry.Shared.Infrastructure.ManagedIdentity;

namespace IoTTelemetry.Shared.Infrastructure.Tests.ManagedIdentity;

public class ManagedIdentityOptionsTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultStrategy()
    {
        // Act
        var options = new ManagedIdentityOptions();

        // Assert
        options.Strategy.Should().Be("Default");
        options.ClientId.Should().BeNull();
    }

    [Fact]
    public void SectionName_ShouldReturnCorrectValue()
    {
        // Assert
        ManagedIdentityOptions.SectionName.Should().Be("AzureManagedIdentity");
    }

    [Theory]
    [InlineData("Default")]
    [InlineData("ManagedIdentityOnly")]
    [InlineData("Chained")]
    [InlineData("Development")]
    public void IsValid_WithValidStrategy_ShouldReturnTrue(string strategy)
    {
        // Arrange
        var options = new ManagedIdentityOptions { Strategy = strategy };

        // Act
        var result = options.IsValid(out var errorMessage);

        // Assert
        result.Should().BeTrue();
        errorMessage.Should().BeNull();
    }

    [Theory]
    [InlineData("default", "Default")]
    [InlineData("MANAGEDIDENTITYONLY", "ManagedIdentityOnly")]
    [InlineData("chained", "Chained")]
    [InlineData("DEVELOPMENT", "Development")]
    public void IsValid_WithValidStrategyCaseInsensitive_ShouldReturnTrue(string strategy, string normalizedStrategy)
    {
        // Arrange
        var options = new ManagedIdentityOptions { Strategy = strategy };

        // Act
        var result = options.IsValid(out var errorMessage);

        // Assert - Verify case-insensitive validation works for all strategies
        result.Should().BeTrue($"Strategy '{strategy}' should be valid (normalized to '{normalizedStrategy}')");
        errorMessage.Should().BeNull();
    }

    [Fact]
    public void IsValid_WithNullStrategy_ShouldReturnFalse()
    {
        // Arrange
        var options = new ManagedIdentityOptions { Strategy = null! };

        // Act
        var result = options.IsValid(out var errorMessage);

        // Assert
        result.Should().BeFalse();
        errorMessage.Should().Be("Strategy cannot be null or empty");
    }

    [Fact]
    public void IsValid_WithEmptyStrategy_ShouldReturnFalse()
    {
        // Arrange
        var options = new ManagedIdentityOptions { Strategy = string.Empty };

        // Act
        var result = options.IsValid(out var errorMessage);

        // Assert
        result.Should().BeFalse();
        errorMessage.Should().Be("Strategy cannot be null or empty");
    }

    [Fact]
    public void IsValid_WithWhitespaceStrategy_ShouldReturnFalse()
    {
        // Arrange
        var options = new ManagedIdentityOptions { Strategy = "   " };

        // Act
        var result = options.IsValid(out var errorMessage);

        // Assert
        result.Should().BeFalse();
        errorMessage.Should().Be("Strategy cannot be null or empty");
    }

    [Fact]
    public void IsValid_WithInvalidStrategy_ShouldReturnFalse()
    {
        // Arrange
        var options = new ManagedIdentityOptions { Strategy = "InvalidStrategy" };

        // Act
        var result = options.IsValid(out var errorMessage);

        // Assert
        result.Should().BeFalse();
        errorMessage.Should().Contain("Strategy must be one of:");
        errorMessage.Should().Contain("Default");
        errorMessage.Should().Contain("ManagedIdentityOnly");
        errorMessage.Should().Contain("Chained");
        errorMessage.Should().Contain("Development");
    }

    [Fact]
    public void ClientId_CanBeSet_ShouldUpdateValue()
    {
        // Arrange
        var options = new ManagedIdentityOptions();
        var clientId = "test-client-id-123";

        // Act
        options.ClientId = clientId;

        // Assert
        options.ClientId.Should().Be(clientId);
    }

    [Fact]
    public void Strategy_CanBeSet_ShouldUpdateValue()
    {
        // Arrange
        var options = new ManagedIdentityOptions();

        // Act
        options.Strategy = "Chained";

        // Assert
        options.Strategy.Should().Be("Chained");
    }
}
