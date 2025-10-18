using IoTTelemetry.Domain.Common;

namespace IoTTelemetry.Domain.ValueObjects;

/// <summary>
/// Represents a UTC timestamp for telemetry events.
/// </summary>
public sealed class Timestamp : ValueObject
{
    public DateTimeOffset Value { get; }

    private Timestamp(DateTimeOffset value)
    {
        Value = value.ToUniversalTime();
    }

    public static Timestamp Create(DateTimeOffset value)
    {
        if (value > DateTimeOffset.UtcNow.AddHours(1))
        {
            throw new ArgumentException("Timestamp cannot be in the future (> 1 hour tolerance).", nameof(value));
        }

        if (value < DateTimeOffset.UtcNow.AddYears(-10))
        {
            throw new ArgumentException("Timestamp cannot be older than 10 years.", nameof(value));
        }

        return new Timestamp(value);
    }

    public static Timestamp Now() => new(DateTimeOffset.UtcNow);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString("O"); // ISO 8601 format
}
