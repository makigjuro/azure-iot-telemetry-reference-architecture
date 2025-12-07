using IoTTelemetry.Domain.ValueObjects;

namespace AlertHandler.Application.Commands;

/// <summary>
/// Command to send a Cloud-to-Device command via IoT Hub.
/// </summary>
public sealed record SendDeviceCommandCommand(
    DeviceId DeviceId,
    string CommandName,
    Dictionary<string, object> Payload,
    Guid AlertId);
