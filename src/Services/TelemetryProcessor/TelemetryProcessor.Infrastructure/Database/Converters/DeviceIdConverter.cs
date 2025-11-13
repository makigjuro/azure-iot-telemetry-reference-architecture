using IoTTelemetry.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TelemetryProcessor.Infrastructure.Database.Converters;

/// <summary>
/// EF Core value converter for DeviceId value object.
/// </summary>
public sealed class DeviceIdConverter : ValueConverter<DeviceId, string>
{
    public DeviceIdConverter()
        : base(
            deviceId => deviceId.Value,
            value => DeviceId.Create(value))
    {
    }
}
