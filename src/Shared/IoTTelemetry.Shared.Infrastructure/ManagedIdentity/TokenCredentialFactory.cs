using Azure.Core;
using Azure.Identity;

namespace IoTTelemetry.Shared.Infrastructure.ManagedIdentity;

/// <summary>
/// Factory for creating Azure token credentials with sensible defaults.
/// Provides different credential strategies for development and production scenarios.
/// </summary>
public static class TokenCredentialFactory
{
    /// <summary>
    /// Creates a DefaultAzureCredential with optimized settings for Azure services.
    /// Supports both local development (Visual Studio, Azure CLI, etc.) and production (Managed Identity).
    /// </summary>
    /// <param name="clientId">Optional client ID for user-assigned managed identity</param>
    /// <returns>TokenCredential configured for the current environment</returns>
    public static TokenCredential CreateDefault(string? clientId = null)
    {
        var options = new DefaultAzureCredentialOptions
        {
            // Exclude shared token cache to avoid credential conflicts in containerized environments
            ExcludeSharedTokenCacheCredential = true,

            // Try managed identity first in production environments
            ManagedIdentityClientId = clientId
        };

        return new DefaultAzureCredential(options);
    }

    /// <summary>
    /// Creates a ManagedIdentityCredential for production environments.
    /// Use this when you explicitly want to use only managed identity (recommended for production).
    /// </summary>
    /// <param name="clientId">Optional client ID for user-assigned managed identity</param>
    /// <returns>TokenCredential using managed identity</returns>
    public static TokenCredential CreateManagedIdentity(string? clientId = null)
    {
        return string.IsNullOrEmpty(clientId)
            ? new ManagedIdentityCredential()
            : new ManagedIdentityCredential(clientId);
    }

    /// <summary>
    /// Creates a ChainedTokenCredential that tries multiple authentication methods in order.
    /// Useful for hybrid environments where you want explicit control over the authentication chain.
    /// </summary>
    /// <param name="clientId">Optional client ID for user-assigned managed identity</param>
    /// <returns>TokenCredential with custom authentication chain</returns>
    public static TokenCredential CreateChained(string? clientId = null)
    {
        var credentials = new List<TokenCredential>
        {
            // Try managed identity first (production)
            CreateManagedIdentity(clientId),

            // Fall back to Azure CLI (local development)
            new AzureCliCredential(),

            // Fall back to environment variables (CI/CD, local development)
            new EnvironmentCredential()
        };

        return new ChainedTokenCredential(credentials.ToArray());
    }

    /// <summary>
    /// Creates a credential specifically for local development.
    /// Uses Azure CLI, environment variables, and interactive browser login.
    /// </summary>
    /// <returns>TokenCredential optimized for local development</returns>
    public static TokenCredential CreateForDevelopment()
    {
        var credentials = new List<TokenCredential>
        {
            // Try Azure CLI first (most common for local dev)
            new AzureCliCredential(),

            // Try environment variables (Docker, local env setup)
            new EnvironmentCredential(),

            // Fall back to interactive browser (last resort)
            new InteractiveBrowserCredential()
        };

        return new ChainedTokenCredential(credentials.ToArray());
    }
}
