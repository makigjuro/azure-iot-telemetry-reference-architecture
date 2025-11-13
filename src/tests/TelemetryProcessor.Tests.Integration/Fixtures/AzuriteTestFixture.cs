using Azure.Storage.Blobs;
using Testcontainers.Azurite;

namespace TelemetryProcessor.Tests.Integration.Fixtures;

/// <summary>
/// Test fixture for Azurite Testcontainer (Azure Storage emulator).
/// Provides blob storage for ADLS Gen2 testing.
/// </summary>
public sealed class AzuriteTestFixture : IAsyncLifetime
{
    private readonly AzuriteContainer _azuriteContainer;

    public AzuriteTestFixture()
    {
        _azuriteContainer = new AzuriteBuilder()
            .WithImage("mcr.microsoft.com/azure-storage/azurite:latest")
            .Build();
    }

    public string ConnectionString => _azuriteContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _azuriteContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _azuriteContainer.DisposeAsync();
    }

    /// <summary>
    /// Creates a BlobServiceClient connected to the Azurite container.
    /// </summary>
    public BlobServiceClient CreateBlobServiceClient()
    {
        return new BlobServiceClient(ConnectionString);
    }

    /// <summary>
    /// Creates a BlobContainerClient for the specified container.
    /// </summary>
    public async Task<BlobContainerClient> CreateContainerClientAsync(string containerName)
    {
        var serviceClient = CreateBlobServiceClient();
        var containerClient = serviceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();
        return containerClient;
    }
}
