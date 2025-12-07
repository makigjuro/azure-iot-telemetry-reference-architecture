namespace AlertHandler.Infrastructure.IoTHub;

/// <summary>
/// Configuration options for Azure IoT Hub.
/// </summary>
public sealed class IoTHubOptions
{
    public const string SectionName = "IoTHub";

    /// <summary>
    /// IoT Hub hostname (e.g., iot-hub-dev-mg123.azure-devices.net)
    /// </summary>
    public string Hostname { get; set; } = string.Empty;

    /// <summary>
    /// Default C2D message time-to-live in seconds
    /// </summary>
    public int DefaultMessageTtlSeconds { get; set; } = 3600;

    /// <summary>
    /// C2D command timeout in seconds
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 30;
}
