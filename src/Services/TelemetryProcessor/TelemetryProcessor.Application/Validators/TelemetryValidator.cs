using IoTTelemetry.Domain.Entities;
using IoTTelemetry.Domain.ValueObjects;
using TelemetryProcessor.Application.Ports;

namespace TelemetryProcessor.Application.Validators;

/// <summary>
/// Default telemetry validator implementation.
/// Validates telemetry readings against business rules.
/// </summary>
public sealed class TelemetryValidator : ITelemetryValidator
{
    private const int MaxMeasurements = 100;
    private const int MaxTelemetryAgeHours = 24;

    public Task<ValidationResult> ValidateAsync(TelemetryReading reading)
    {
        // Check for bad quality measurements
        if (reading.HasBadQuality())
        {
            return Task.FromResult(
                ValidationResult.Failure("Telemetry contains bad quality measurements"));
        }

        // Check measurement count
        if (reading.Measurements.Count > MaxMeasurements)
        {
            return Task.FromResult(
                ValidationResult.Failure(
                    $"Telemetry exceeds maximum of {MaxMeasurements} measurements"));
        }

        // Check telemetry age (reject stale data)
        var age = reading.GetAge();
        if (age.TotalHours > MaxTelemetryAgeHours)
        {
            return Task.FromResult(
                ValidationResult.Failure(
                    $"Telemetry is too old ({age.TotalHours:F1} hours, max {MaxTelemetryAgeHours} hours)"));
        }

        // Check for future timestamps
        if (reading.Timestamp.Value > DateTimeOffset.UtcNow.AddMinutes(5))
        {
            return Task.FromResult(
                ValidationResult.Failure("Telemetry timestamp is in the future"));
        }

        return Task.FromResult(ValidationResult.Success());
    }
}
