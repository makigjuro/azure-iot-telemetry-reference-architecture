using IoTTelemetry.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TelemetryProcessor.Infrastructure.Database.Converters;

/// <summary>
/// EF Core value converter for Timestamp value object.
/// </summary>
public sealed class TimestampConverter : ValueConverter<Timestamp, DateTimeOffset>
{
    public TimestampConverter()
        : base(
            timestamp => timestamp.Value,
            value => Timestamp.Create(value))
    {
    }
}
