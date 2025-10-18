using IoTTelemetry.Domain.ValueObjects;

namespace IoTTelemetry.Domain.Tests.ValueObjects;

public class TelemetryValueTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        const double value = 25.5;
        const string unit = "°C";

        // Act
        var telemetryValue = TelemetryValue.Create(value, unit);

        // Assert
        telemetryValue.Value.Should().Be(value);
        telemetryValue.Unit.Should().Be(unit);
        telemetryValue.Quality.Should().Be(TelemetryQuality.Good);
    }

    [Fact]
    public void Create_WithSpecifiedQuality_ShouldUseProvidedQuality()
    {
        // Act
        var telemetryValue = TelemetryValue.Create(100.0, "kPa", TelemetryQuality.Uncertain);

        // Assert
        telemetryValue.Quality.Should().Be(TelemetryQuality.Uncertain);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyUnit_ShouldThrowArgumentException(string invalidUnit)
    {
        // Act
        var act = () => TelemetryValue.Create(10.0, invalidUnit!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Unit cannot be empty*");
    }

    [Fact]
    public void Create_WithNaNValue_ShouldThrowArgumentException()
    {
        // Act
        var act = () => TelemetryValue.Create(double.NaN, "m/s");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Value must be a valid number*");
    }

    [Fact]
    public void Create_WithInfinityValue_ShouldThrowArgumentException()
    {
        // Act
        var act = () => TelemetryValue.Create(double.PositiveInfinity, "m/s");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Value must be a valid number*");
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var value1 = TelemetryValue.Create(25.5, "°C", TelemetryQuality.Good);
        var value2 = TelemetryValue.Create(25.5, "°C", TelemetryQuality.Good);

        // Assert
        value1.Should().Be(value2);
        value1.GetHashCode().Should().Be(value2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var value1 = TelemetryValue.Create(25.5, "°C");
        var value2 = TelemetryValue.Create(26.0, "°C");

        // Assert
        value1.Should().NotBe(value2);
    }

    [Fact]
    public void Equality_WithDifferentUnits_ShouldNotBeEqual()
    {
        // Arrange
        var value1 = TelemetryValue.Create(25.5, "°C");
        var value2 = TelemetryValue.Create(25.5, "°F");

        // Assert
        value1.Should().NotBe(value2);
    }

    [Fact]
    public void Equality_WithDifferentQuality_ShouldNotBeEqual()
    {
        // Arrange
        var value1 = TelemetryValue.Create(25.5, "°C", TelemetryQuality.Good);
        var value2 = TelemetryValue.Create(25.5, "°C", TelemetryQuality.Bad);

        // Assert
        value1.Should().NotBe(value2);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var telemetryValue = TelemetryValue.Create(25.5, "°C", TelemetryQuality.Good);

        // Act
        var result = telemetryValue.ToString();

        // Assert
        result.Should().Contain("°C");
        result.Should().Contain("Good");
        result.Should().Contain("25");
    }
}
