using IoTTelemetry.Domain.Entities;
using IoTTelemetry.Domain.ValueObjects;
using TelemetryProcessor.Application.Validators;

namespace TelemetryProcessor.Application.Tests.Validators;

public class TelemetryValidatorTests
{
    private readonly TelemetryValidator _validator;

    public TelemetryValidatorTests()
    {
        _validator = new TelemetryValidator();
    }

    [Fact]
    public async Task ValidateAsync_WithValidTelemetry_ShouldReturnSuccess()
    {
        // Arrange
        var reading = CreateValidReading();

        // Act
        var result = await _validator.ValidateAsync(reading);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAsync_WithBadQualityMeasurements_ShouldReturnFailure()
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
        var result = await _validator.ValidateAsync(reading);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().Be("Telemetry contains bad quality measurements");
    }

    [Fact]
    public async Task ValidateAsync_WithTooManyMeasurements_ShouldReturnFailure()
    {
        // Arrange
        var measurements = new Dictionary<string, TelemetryValue>();
        for (int i = 0; i < 101; i++)
        {
            measurements[$"measurement_{i}"] = TelemetryValue.Create(i, "unit");
        }
        var reading = TelemetryReading.Create(
            DeviceId.Create("device-001"),
            Timestamp.Now(),
            measurements);

        // Act
        var result = await _validator.ValidateAsync(reading);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().Be("Telemetry exceeds maximum of 100 measurements");
    }

    [Fact]
    public async Task ValidateAsync_WithExactlyMaxMeasurements_ShouldReturnSuccess()
    {
        // Arrange
        var measurements = new Dictionary<string, TelemetryValue>();
        for (int i = 0; i < 100; i++)
        {
            measurements[$"measurement_{i}"] = TelemetryValue.Create(i, "unit");
        }
        var reading = TelemetryReading.Create(
            DeviceId.Create("device-001"),
            Timestamp.Now(),
            measurements);

        // Act
        var result = await _validator.ValidateAsync(reading);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WithStaleTelemetry_ShouldReturnFailure()
    {
        // Arrange - Create telemetry from 25 hours ago (exceeds 24h limit)
        var oldTimestamp = Timestamp.Create(DateTimeOffset.UtcNow.AddHours(-25));
        var reading = TelemetryReading.Create(
            DeviceId.Create("device-001"),
            oldTimestamp,
            new Dictionary<string, TelemetryValue>
            {
                ["temperature"] = TelemetryValue.Create(25.5, "°C")
            });

        // Act
        var result = await _validator.ValidateAsync(reading);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("too old");
        result.Error.Should().Contain("25");
        result.Error.Should().Contain("24");
    }

    [Fact]
    public async Task ValidateAsync_WithTelemetryJustUnderMaxAge_ShouldReturnSuccess()
    {
        // Arrange - Create telemetry from 23.5 hours ago (well within 24h limit)
        var timestamp = Timestamp.Create(DateTimeOffset.UtcNow.AddHours(-23.5));
        var reading = TelemetryReading.Create(
            DeviceId.Create("device-001"),
            timestamp,
            new Dictionary<string, TelemetryValue>
            {
                ["temperature"] = TelemetryValue.Create(25.5, "°C")
            });

        // Act
        var result = await _validator.ValidateAsync(reading);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WithFutureTimestamp_ShouldReturnFailure()
    {
        // Arrange - Create telemetry from 10 minutes in the future (exceeds 5 min buffer)
        var futureTimestamp = Timestamp.Create(DateTimeOffset.UtcNow.AddMinutes(10));
        var reading = TelemetryReading.Create(
            DeviceId.Create("device-001"),
            futureTimestamp,
            new Dictionary<string, TelemetryValue>
            {
                ["temperature"] = TelemetryValue.Create(25.5, "°C")
            });

        // Act
        var result = await _validator.ValidateAsync(reading);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().Be("Telemetry timestamp is in the future");
    }

    [Fact]
    public async Task ValidateAsync_WithTimestampJustWithinFutureBuffer_ShouldReturnSuccess()
    {
        // Arrange - Create telemetry from 4 minutes in the future (within 5 min buffer)
        var futureTimestamp = Timestamp.Create(DateTimeOffset.UtcNow.AddMinutes(4));
        var reading = TelemetryReading.Create(
            DeviceId.Create("device-001"),
            futureTimestamp,
            new Dictionary<string, TelemetryValue>
            {
                ["temperature"] = TelemetryValue.Create(25.5, "°C")
            });

        // Act
        var result = await _validator.ValidateAsync(reading);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WithRecentValidTelemetry_ShouldReturnSuccess()
    {
        // Arrange - Create telemetry from 1 hour ago
        var timestamp = Timestamp.Create(DateTimeOffset.UtcNow.AddHours(-1));
        var reading = TelemetryReading.Create(
            DeviceId.Create("device-001"),
            timestamp,
            new Dictionary<string, TelemetryValue>
            {
                ["temperature"] = TelemetryValue.Create(25.5, "°C"),
                ["humidity"] = TelemetryValue.Create(60.0, "%")
            });

        // Act
        var result = await _validator.ValidateAsync(reading);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    private static TelemetryReading CreateValidReading()
    {
        var measurements = new Dictionary<string, TelemetryValue>
        {
            ["temperature"] = TelemetryValue.Create(25.5, "°C", TelemetryQuality.Good),
            ["humidity"] = TelemetryValue.Create(60.0, "%", TelemetryQuality.Good)
        };

        return TelemetryReading.Create(
            DeviceId.Create("device-001"),
            Timestamp.Now(),
            measurements);
    }
}
