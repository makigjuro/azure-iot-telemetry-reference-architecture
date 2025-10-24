using IoTTelemetry.Domain.Entities;

namespace TelemetryProcessor.Application.Ports;

/// <summary>
/// Port for validating telemetry readings.
/// </summary>
public interface ITelemetryValidator
{
    /// <summary>
    /// Validates a telemetry reading.
    /// </summary>
    /// <param name="reading">Telemetry reading to validate</param>
    /// <returns>Validation result with errors if invalid</returns>
    Task<ValidationResult> ValidateAsync(TelemetryReading reading);
}

/// <summary>
/// Validation result.
/// </summary>
public sealed record ValidationResult(bool IsValid, string? Error = null)
{
    public static ValidationResult Success() => new(true);
    public static ValidationResult Failure(string error) => new(false, error);
}
