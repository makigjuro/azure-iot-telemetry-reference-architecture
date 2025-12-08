namespace EventSubscriber.Application.Commands;

/// <summary>
/// Command triggered when a device is created in IoT Hub.
/// Received from Event Grid webhook.
/// </summary>
public sealed record DeviceCreatedCommand(
    string DeviceId,
    string EventType,
    DateTimeOffset EventTime,
    Dictionary<string, object>? Data);
