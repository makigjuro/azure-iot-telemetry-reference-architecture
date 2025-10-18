using IoTTelemetry.Domain.Common;

namespace IoTTelemetry.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identifier for devices.
/// </summary>
public sealed class DeviceId : ValueObject
{
    public string Value { get; }

    private DeviceId(string value)
    {
        Value = value;
    }

    public static DeviceId Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Device ID cannot be empty.", nameof(value));
        }

        if (value.Length > 128)
        {
            throw new ArgumentException("Device ID cannot exceed 128 characters.", nameof(value));
        }

        // Azure IoT Hub device ID constraints
        if (!IsValidDeviceId(value))
        {
            throw new ArgumentException(
                "Device ID can only contain alphanumeric characters, '-', '.', '_', and ':'.",
                nameof(value));
        }

        return new DeviceId(value);
    }

    private static bool IsValidDeviceId(string value)
    {
        return value.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '.' || c == '_' || c == ':');
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
