using Azure.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IoTTelemetry.Shared.Infrastructure.ManagedIdentity;

/// <summary>
/// Extension methods for registering Azure Managed Identity services in the DI container.
/// </summary>
public static class ManagedIdentityExtensions
{
    /// <summary>
    /// Adds Azure Managed Identity support with DefaultAzureCredential.
    /// This is the recommended approach for most scenarios as it works in both development and production.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="clientId">Optional client ID for user-assigned managed identity</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAzureManagedIdentity(
        this IServiceCollection services,
        string? clientId = null)
    {
        services.AddSingleton<TokenCredential>(_ => TokenCredentialFactory.CreateDefault(clientId));
        services.AddLogging();
        services.AddSingleton<AzureClientFactory>();

        return services;
    }

    /// <summary>
    /// Adds Azure Managed Identity support with explicit managed identity credential.
    /// Use this for production environments where you want to ensure only managed identity is used.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="clientId">Optional client ID for user-assigned managed identity</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAzureManagedIdentityOnly(
        this IServiceCollection services,
        string? clientId = null)
    {
        services.AddSingleton<TokenCredential>(_ => TokenCredentialFactory.CreateManagedIdentity(clientId));
        services.AddLogging();
        services.AddSingleton<AzureClientFactory>();

        return services;
    }

    /// <summary>
    /// Adds Azure Managed Identity support with a chained credential strategy.
    /// Provides explicit control over the authentication chain order.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="clientId">Optional client ID for user-assigned managed identity</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAzureChainedCredential(
        this IServiceCollection services,
        string? clientId = null)
    {
        services.AddSingleton<TokenCredential>(_ => TokenCredentialFactory.CreateChained(clientId));
        services.AddLogging();
        services.AddSingleton<AzureClientFactory>();

        return services;
    }

    /// <summary>
    /// Adds Azure Managed Identity support optimized for local development.
    /// Uses Azure CLI, environment variables, and interactive browser login.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAzureDevelopmentCredential(this IServiceCollection services)
    {
        services.AddSingleton<TokenCredential>(_ => TokenCredentialFactory.CreateForDevelopment());
        services.AddLogging();
        services.AddSingleton<AzureClientFactory>();

        return services;
    }

    /// <summary>
    /// Adds Azure Managed Identity support based on configuration.
    /// Reads settings from IConfiguration to determine which credential strategy to use.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <param name="configSection">The configuration section containing managed identity settings (default: "AzureManagedIdentity")</param>
    /// <returns>The service collection for chaining</returns>
    /// <example>
    /// Configuration example (appsettings.json):
    /// {
    ///   "AzureManagedIdentity": {
    ///     "Strategy": "Default",  // Options: Default, ManagedIdentityOnly, Chained, Development
    ///     "ClientId": null        // Optional: specify for user-assigned managed identity
    ///   }
    /// }
    /// </example>
    public static IServiceCollection AddAzureManagedIdentityFromConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSection = "AzureManagedIdentity")
    {
        var config = configuration.GetSection(configSection);
        var strategy = config.GetValue<string>("Strategy") ?? "Default";
        var clientId = config.GetValue<string?>("ClientId");

        return strategy.ToLowerInvariant() switch
        {
            "managedidentityonly" => services.AddAzureManagedIdentityOnly(clientId),
            "chained" => services.AddAzureChainedCredential(clientId),
            "development" => services.AddAzureDevelopmentCredential(),
            _ => services.AddAzureManagedIdentity(clientId)
        };
    }

    /// <summary>
    /// Registers a strongly-typed Azure SDK client in the DI container.
    /// The client will be created using the registered AzureClientFactory.
    /// </summary>
    /// <typeparam name="TClient">The type of Azure SDK client to register</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="factory">Factory function that takes a TokenCredential and returns the client</param>
    /// <returns>The service collection for chaining</returns>
    /// <example>
    /// services.AddAzureClient&lt;EventHubProducerClient&gt;(
    ///     credential => new EventHubProducerClient(fullyQualifiedNamespace, eventHubName, credential)
    /// );
    /// </example>
    public static IServiceCollection AddAzureClient<TClient>(
        this IServiceCollection services,
        Func<TokenCredential, TClient> factory)
        where TClient : class
    {
        services.AddSingleton(sp =>
        {
            var clientFactory = sp.GetRequiredService<AzureClientFactory>();
            return clientFactory.CreateClient(factory);
        });

        return services;
    }

    /// <summary>
    /// Registers a strongly-typed Azure SDK client with custom options in the DI container.
    /// The client will be created using the registered AzureClientFactory.
    /// </summary>
    /// <typeparam name="TClient">The type of Azure SDK client to register</typeparam>
    /// <typeparam name="TOptions">The type of client options</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="factory">Factory function that takes a TokenCredential and options and returns the client</param>
    /// <param name="optionsConfigurator">Action to configure client options</param>
    /// <returns>The service collection for chaining</returns>
    /// <example>
    /// services.AddAzureClientWithOptions&lt;BlobServiceClient, BlobClientOptions&gt;(
    ///     (credential, options) => new BlobServiceClient(new Uri(blobUri), credential, options),
    ///     options => options.Retry.MaxRetries = 5
    /// );
    /// </example>
    public static IServiceCollection AddAzureClientWithOptions<TClient, TOptions>(
        this IServiceCollection services,
        Func<TokenCredential, TOptions, TClient> factory,
        Action<TOptions>? optionsConfigurator = null)
        where TClient : class
        where TOptions : class, new()
    {
        services.AddSingleton(sp =>
        {
            var clientFactory = sp.GetRequiredService<AzureClientFactory>();
            return clientFactory.CreateClientWithOptions(factory, optionsConfigurator);
        });

        return services;
    }
}
