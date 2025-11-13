using IoTTelemetry.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TelemetryProcessor.Infrastructure.Database.Converters;

/// <summary>
/// EF Core value converter for nullable Timestamp value object.
/// </summary>
public sealed class NullableTimestampConverter : ValueConverter<Timestamp?, DateTimeOffset?>
{
    public NullableTimestampConverter()
        : base(
            timestamp => timestamp != null ? timestamp.Value : (DateTimeOffset?)null,
            value => value.HasValue ? Timestamp.Create(value.Value) : null)
    {
    }
}
