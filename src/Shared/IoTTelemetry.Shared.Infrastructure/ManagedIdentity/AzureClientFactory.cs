using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;

namespace IoTTelemetry.Shared.Infrastructure.ManagedIdentity;

/// <summary>
/// Factory for creating Azure SDK clients with managed identity authentication.
/// Provides a consistent way to instantiate Azure service clients across the application.
/// </summary>
public sealed class AzureClientFactory
{
    private readonly TokenCredential _credential;
    private readonly ILogger<AzureClientFactory> _logger;

    /// <summary>
    /// Initializes a new instance of AzureClientFactory with the specified credential.
    /// </summary>
    /// <param name="credential">The token credential to use for authentication</param>
    /// <param name="logger">Logger for diagnostic information</param>
    public AzureClientFactory(TokenCredential credential, ILogger<AzureClientFactory> logger)
    {
        _credential = credential ?? throw new ArgumentNullException(nameof(credential));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates an Azure SDK client of type TClient using the configured credential.
    /// </summary>
    /// <typeparam name="TClient">The type of Azure SDK client to create</typeparam>
    /// <param name="factory">Factory function that takes a credential and returns the client</param>
    /// <returns>Configured Azure SDK client</returns>
    /// <example>
    /// var eventHubClient = factory.CreateClient(
    ///     credential => new EventHubProducerClient(fullyQualifiedNamespace, eventHubName, credential)
    /// );
    /// </example>
    public TClient CreateClient<TClient>(Func<TokenCredential, TClient> factory)
        where TClient : class
    {
        ArgumentNullException.ThrowIfNull(factory);

        try
        {
            var client = factory(_credential);
            _logger.LogDebug("Successfully created Azure client of type {ClientType}", typeof(TClient).Name);
            return client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Azure client of type {ClientType}", typeof(TClient).Name);
            throw new InvalidOperationException($"Failed to create Azure client of type {typeof(TClient).Name}", ex);
        }
    }

    /// <summary>
    /// Creates an Azure SDK client with additional configuration options.
    /// </summary>
    /// <typeparam name="TClient">The type of Azure SDK client to create</typeparam>
    /// <typeparam name="TOptions">The type of client options</typeparam>
    /// <param name="factory">Factory function that takes a credential and options and returns the client</param>
    /// <param name="optionsConfigurator">Action to configure client options</param>
    /// <returns>Configured Azure SDK client</returns>
    /// <example>
    /// var blobClient = factory.CreateClientWithOptions&lt;BlobServiceClient, BlobClientOptions&gt;(
    ///     (credential, options) => new BlobServiceClient(new Uri(blobUri), credential, options),
    ///     options => options.Retry.MaxRetries = 5
    /// );
    /// </example>
    public TClient CreateClientWithOptions<TClient, TOptions>(
        Func<TokenCredential, TOptions, TClient> factory,
        Action<TOptions>? optionsConfigurator = null)
        where TClient : class
        where TOptions : class, new()
    {
        ArgumentNullException.ThrowIfNull(factory);

        try
        {
            var options = new TOptions();
            optionsConfigurator?.Invoke(options);

            var client = factory(_credential, options);
            _logger.LogDebug(
                "Successfully created Azure client of type {ClientType} with options {OptionsType}",
                typeof(TClient).Name,
                typeof(TOptions).Name);

            return client;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create Azure client of type {ClientType} with options {OptionsType}",
                typeof(TClient).Name,
                typeof(TOptions).Name);
            throw new InvalidOperationException(
                $"Failed to create Azure client of type {typeof(TClient).Name} with options {typeof(TOptions).Name}",
                ex);
        }
    }

    /// <summary>
    /// Gets the underlying token credential used by this factory.
    /// </summary>
    public TokenCredential Credential => _credential;
}
