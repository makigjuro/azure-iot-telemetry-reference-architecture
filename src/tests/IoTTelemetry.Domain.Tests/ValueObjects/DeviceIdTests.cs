using IoTTelemetry.Domain.ValueObjects;

namespace IoTTelemetry.Domain.Tests.ValueObjects;

public class DeviceIdTests
{
    [Fact]
    public void Create_WithValidId_ShouldSucceed()
    {
        // Arrange
        var validId = "device-001";

        // Act
        var deviceId = DeviceId.Create(validId);

        // Assert
        deviceId.Value.Should().Be(validId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyId_ShouldThrowArgumentException(string invalidId)
    {
        // Act
        var act = () => DeviceId.Create(invalidId!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Device ID cannot be empty*");
    }

    [Fact]
    public void Create_WithTooLongId_ShouldThrowArgumentException()
    {
        // Arrange
        var longId = new string('a', 129);

        // Act
        var act = () => DeviceId.Create(longId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Device ID cannot exceed 128 characters*");
    }

    [Theory]
    [InlineData("device@001")]
    [InlineData("device#001")]
    [InlineData("device 001")]
    [InlineData("device!001")]
    public void Create_WithInvalidCharacters_ShouldThrowArgumentException(string invalidId)
    {
        // Act
        var act = () => DeviceId.Create(invalidId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*can only contain alphanumeric characters*");
    }

    [Theory]
    [InlineData("device-001")]
    [InlineData("device.001")]
    [InlineData("device_001")]
    [InlineData("device:001")]
    [InlineData("Device123")]
    public void Create_WithValidCharacters_ShouldSucceed(string validId)
    {
        // Act
        var deviceId = DeviceId.Create(validId);

        // Assert
        deviceId.Value.Should().Be(validId);
    }

    [Fact]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var id1 = DeviceId.Create("device-001");
        var id2 = DeviceId.Create("device-001");

        // Assert
        id1.Should().Be(id2);
        (id1 == id2).Should().BeTrue();
        id1.GetHashCode().Should().Be(id2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var id1 = DeviceId.Create("device-001");
        var id2 = DeviceId.Create("device-002");

        // Assert
        id1.Should().NotBe(id2);
        (id1 != id2).Should().BeTrue();
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var deviceId = DeviceId.Create("device-001");

        // Act
        var result = deviceId.ToString();

        // Assert
        result.Should().Be("device-001");
    }
}
