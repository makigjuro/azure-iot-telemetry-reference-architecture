using IoTTelemetry.Domain.Common;

namespace IoTTelemetry.Domain.ValueObjects;

/// <summary>
/// Represents a telemetry measurement value with unit and quality indicator.
/// </summary>
public sealed class TelemetryValue : ValueObject
{
    public double Value { get; }
    public string Unit { get; }
    public TelemetryQuality Quality { get; }

    private TelemetryValue(double value, string unit, TelemetryQuality quality)
    {
        Value = value;
        Unit = unit;
        Quality = quality;
    }

    public static TelemetryValue Create(double value, string unit, TelemetryQuality quality = TelemetryQuality.Good)
    {
        if (string.IsNullOrWhiteSpace(unit))
        {
            throw new ArgumentException("Unit cannot be empty.", nameof(unit));
        }

        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentException("Value must be a valid number.", nameof(value));
        }

        return new TelemetryValue(value, unit, quality);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
        yield return Unit;
        yield return Quality;
    }

    public override string ToString() => $"{Value} {Unit} ({Quality})";
}

/// <summary>
/// Quality indicator for telemetry measurements.
/// </summary>
public enum TelemetryQuality
{
    /// <summary>
    /// Good quality measurement.
    /// </summary>
    Good = 0,

    /// <summary>
    /// Uncertain quality - may be inaccurate.
    /// </summary>
    Uncertain = 1,

    /// <summary>
    /// Bad quality - should not be used for analysis.
    /// </summary>
    Bad = 2
}
