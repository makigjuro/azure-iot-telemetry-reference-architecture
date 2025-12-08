namespace EventSubscriber.Infrastructure.DigitalTwins;

/// <summary>
/// Configuration options for Azure Digital Twins connection.
/// </summary>
public sealed class DigitalTwinOptions
{
    public const string SectionName = "DigitalTwins";

    /// <summary>
    /// Azure Digital Twins instance URL (e.g., https://myinstance.api.weu.digitaltwins.azure.net).
    /// </summary>
    public required string InstanceUrl { get; init; }

    /// <summary>
    /// Model ID for device digital twins.
    /// </summary>
    public string DeviceModelId { get; init; } = "dtmi:com:iot:Device;1";

    /// <summary>
    /// Timeout for Digital Twins operations in seconds.
    /// </summary>
    public int OperationTimeoutSeconds { get; init; } = 30;
}
