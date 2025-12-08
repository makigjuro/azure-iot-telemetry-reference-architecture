namespace EventSubscriber.Application.Commands;

/// <summary>
/// Command triggered when a device is deleted from IoT Hub.
/// Received from Event Grid webhook.
/// </summary>
public sealed record DeviceDeletedCommand(
    string DeviceId,
    string EventType,
    DateTimeOffset EventTime);
