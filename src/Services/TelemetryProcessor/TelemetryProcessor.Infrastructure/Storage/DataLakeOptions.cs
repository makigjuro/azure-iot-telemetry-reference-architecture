namespace TelemetryProcessor.Infrastructure.Storage;

/// <summary>
/// Configuration options for Data Lake storage.
/// </summary>
public sealed class DataLakeOptions
{
    public const string SectionName = "DataLake";

    /// <summary>
    /// Storage account name (e.g., "stiotdevmg123")
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Bronze container name (raw telemetry)
    /// </summary>
    public string BronzeContainer { get; set; } = "bronze";

    /// <summary>
    /// Silver container name (validated + enriched)
    /// </summary>
    public string SilverContainer { get; set; } = "silver";

    /// <summary>
    /// Gold container name (aggregated metrics)
    /// </summary>
    public string GoldContainer { get; set; } = "gold";
}
