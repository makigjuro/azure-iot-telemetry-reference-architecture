using IoTTelemetry.Domain.ValueObjects;

namespace AlertHandler.Application.Ports;

/// <summary>
/// Port for sending Cloud-to-Device commands via Azure IoT Hub.
/// </summary>
public interface IDeviceCommandSender
{
    /// <summary>
    /// Sends a command to a device via IoT Hub.
    /// </summary>
    /// <param name="deviceId">Target device ID</param>
    /// <param name="commandName">Name of the command to execute</param>
    /// <param name="payload">Command payload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if command was successfully delivered</returns>
    Task<bool> SendCommandAsync(
        DeviceId deviceId,
        string commandName,
        Dictionary<string, object> payload,
        CancellationToken cancellationToken = default);
}
