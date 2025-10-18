using IoTTelemetry.Domain.Entities;
using IoTTelemetry.Domain.ValueObjects;

namespace IoTTelemetry.Domain.Tests.Entities;

public class TelemetryReadingTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateReading()
    {
        // Arrange
        var deviceId = DeviceId.Create("device-001");
        var timestamp = Timestamp.Now();
        var measurements = new Dictionary<string, TelemetryValue>
        {
            ["temperature"] = TelemetryValue.Create(25.5, "°C"),
            ["humidity"] = TelemetryValue.Create(60.0, "%")
        };

        // Act
        var reading = TelemetryReading.Create(deviceId, timestamp, measurements);

        // Assert
        reading.DeviceId.Should().Be(deviceId);
        reading.Timestamp.Should().Be(timestamp);
        reading.Measurements.Should().HaveCount(2);
        reading.IsValid.Should().BeTrue();
        reading.ValidationError.Should().BeNull();
        reading.ReceivedAt.Should().NotBeNull();
    }

    [Fact]
    public void Create_WithEmptyMeasurements_ShouldThrowArgumentException()
    {
        // Arrange
        var deviceId = DeviceId.Create("device-001");
        var timestamp = Timestamp.Now();
        var measurements = new Dictionary<string, TelemetryValue>();

        // Act
        var act = () => TelemetryReading.Create(deviceId, timestamp, measurements);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Telemetry reading must have at least one measurement*");
    }

    [Fact]
    public void MarkAsInvalid_WithReason_ShouldSetInvalidState()
    {
        // Arrange
        var reading = CreateValidReading();
        const string reason = "Temperature out of range";

        // Act
        reading.MarkAsInvalid(reason);

        // Assert
        reading.IsValid.Should().BeFalse();
        reading.ValidationError.Should().Be(reason);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void MarkAsInvalid_WithEmptyReason_ShouldThrowArgumentException(string invalidReason)
    {
        // Arrange
        var reading = CreateValidReading();

        // Act
        var act = () => reading.MarkAsInvalid(invalidReason!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Validation error reason cannot be empty*");
    }

    [Fact]
    public void GetMeasurement_WithExistingKey_ShouldReturnValue()
    {
        // Arrange
        var reading = CreateValidReading();

        // Act
        var measurement = reading.GetMeasurement("temperature");

        // Assert
        measurement.Should().NotBeNull();
        measurement!.Value.Should().Be(25.5);
        measurement.Unit.Should().Be("°C");
    }

    [Fact]
    public void GetMeasurement_WithNonExistingKey_ShouldReturnNull()
    {
        // Arrange
        var reading = CreateValidReading();

        // Act
        var measurement = reading.GetMeasurement("pressure");

        // Assert
        measurement.Should().BeNull();
    }

    [Fact]
    public void HasBadQuality_WhenAllGoodQuality_ShouldReturnFalse()
    {
        // Arrange
        var measurements = new Dictionary<string, TelemetryValue>
        {
            ["temperature"] = TelemetryValue.Create(25.5, "°C", TelemetryQuality.Good),
            ["humidity"] = TelemetryValue.Create(60.0, "%", TelemetryQuality.Good)
        };
        var reading = TelemetryReading.Create(
            DeviceId.Create("device-001"),
            Timestamp.Now(),
            measurements);

        // Act
        var hasBadQuality = reading.HasBadQuality();

        // Assert
        hasBadQuality.Should().BeFalse();
    }

    [Fact]
    public void HasBadQuality_WhenAnyBadQuality_ShouldReturnTrue()
    {
        // Arrange
        var measurements = new Dictionary<string, TelemetryValue>
        {
            ["temperature"] = TelemetryValue.Create(25.5, "°C", TelemetryQuality.Good),
            ["humidity"] = TelemetryValue.Create(60.0, "%", TelemetryQuality.Bad)
        };
        var reading = TelemetryReading.Create(
            DeviceId.Create("device-001"),
            Timestamp.Now(),
            measurements);

        // Act
        var hasBadQuality = reading.HasBadQuality();

        // Assert
        hasBadQuality.Should().BeTrue();
    }

    [Fact]
    public void GetAge_ShouldReturnTimeSinceTimestamp()
    {
        // Arrange
        var pastTimestamp = Timestamp.Create(DateTimeOffset.UtcNow.AddMinutes(-5));
        var reading = TelemetryReading.Create(
            DeviceId.Create("device-001"),
            pastTimestamp,
            new Dictionary<string, TelemetryValue>
            {
                ["temperature"] = TelemetryValue.Create(25.5, "°C")
            });

        // Act
        var age = reading.GetAge();

        // Assert
        age.Should().BeGreaterThan(TimeSpan.FromMinutes(4));
        age.Should().BeLessThan(TimeSpan.FromMinutes(6));
    }

    private static TelemetryReading CreateValidReading()
    {
        var measurements = new Dictionary<string, TelemetryValue>
        {
            ["temperature"] = TelemetryValue.Create(25.5, "°C"),
            ["humidity"] = TelemetryValue.Create(60.0, "%")
        };

        return TelemetryReading.Create(
            DeviceId.Create("device-001"),
            Timestamp.Now(),
            measurements);
    }
}
