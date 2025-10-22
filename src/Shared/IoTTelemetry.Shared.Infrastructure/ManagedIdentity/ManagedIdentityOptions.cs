namespace IoTTelemetry.Shared.Infrastructure.ManagedIdentity;

/// <summary>
/// Configuration options for Azure Managed Identity.
/// </summary>
public sealed class ManagedIdentityOptions
{
    /// <summary>
    /// Configuration section name for managed identity settings.
    /// </summary>
    public const string SectionName = "AzureManagedIdentity";

    /// <summary>
    /// The credential strategy to use.
    /// Options: Default, ManagedIdentityOnly, Chained, Development
    /// </summary>
    public string Strategy { get; set; } = "Default";

    /// <summary>
    /// Optional client ID for user-assigned managed identity.
    /// If null or empty, system-assigned managed identity will be used.
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// Validates the options.
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(Strategy))
        {
            errorMessage = "Strategy cannot be null or empty";
            return false;
        }

        var validStrategies = new[] { "Default", "ManagedIdentityOnly", "Chained", "Development" };
        if (!validStrategies.Contains(Strategy, StringComparer.OrdinalIgnoreCase))
        {
            errorMessage = $"Strategy must be one of: {string.Join(", ", validStrategies)}";
            return false;
        }

        errorMessage = null;
        return true;
    }
}
