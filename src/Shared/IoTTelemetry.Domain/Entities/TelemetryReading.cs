using IoTTelemetry.Domain.Common;
using IoTTelemetry.Domain.ValueObjects;

namespace IoTTelemetry.Domain.Entities;

/// <summary>
/// Represents a telemetry reading from a device.
/// </summary>
public sealed class TelemetryReading : Entity<Guid>
{
    private TelemetryReading(
        Guid id,
        DeviceId deviceId,
        Timestamp timestamp,
        Dictionary<string, TelemetryValue> measurements) : base(id)
    {
        DeviceId = deviceId;
        Timestamp = timestamp;
        Measurements = measurements;
        ReceivedAt = Timestamp.Now();
    }

    public DeviceId DeviceId { get; private init; }
    public Timestamp Timestamp { get; private init; }
    public Dictionary<string, TelemetryValue> Measurements { get; private init; }
    public Timestamp ReceivedAt { get; private init; }
    public bool IsValid { get; private set; } = true;
    public string? ValidationError { get; private set; }

    /// <summary>
    /// Creates a new telemetry reading.
    /// </summary>
    public static TelemetryReading Create(
        DeviceId deviceId,
        Timestamp timestamp,
        Dictionary<string, TelemetryValue> measurements)
    {
        if (measurements.Count == 0)
        {
            throw new ArgumentException("Telemetry reading must have at least one measurement.", nameof(measurements));
        }

        var id = Guid.NewGuid();
        return new TelemetryReading(id, deviceId, timestamp, measurements);
    }

    /// <summary>
    /// Marks this reading as invalid with a reason.
    /// </summary>
    public void MarkAsInvalid(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Validation error reason cannot be empty.", nameof(reason));
        }

        IsValid = false;
        ValidationError = reason;
    }

    /// <summary>
    /// Gets a measurement value by key.
    /// </summary>
    public TelemetryValue? GetMeasurement(string key)
    {
        return Measurements.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Checks if reading has measurements with bad quality.
    /// </summary>
    public bool HasBadQuality()
    {
        return Measurements.Values.Any(m => m.Quality == TelemetryQuality.Bad);
    }

    /// <summary>
    /// Gets the age of the reading.
    /// </summary>
    public TimeSpan GetAge()
    {
        return DateTimeOffset.UtcNow - Timestamp.Value;
    }
}
