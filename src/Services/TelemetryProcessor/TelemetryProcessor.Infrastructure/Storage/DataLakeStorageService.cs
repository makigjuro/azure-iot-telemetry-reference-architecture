using System.Text;
using System.Text.Json;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using IoTTelemetry.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TelemetryProcessor.Application.Ports;

namespace TelemetryProcessor.Infrastructure.Storage;

/// <summary>
/// Data Lake storage adapter for medallion architecture (bronze/silver/gold).
/// Uses managed identity for authentication.
/// </summary>
public sealed class DataLakeStorageService : ITelemetryStorage
{
    private readonly DataLakeOptions _options;
    private readonly ILogger<DataLakeStorageService> _logger;
    private readonly BlobServiceClient _blobServiceClient;

    public DataLakeStorageService(
        IOptions<DataLakeOptions> options,
        ILogger<DataLakeStorageService> logger)
    {
        _options = options.Value;
        _logger = logger;

        var serviceUri = new Uri($"https://{_options.AccountName}.blob.core.windows.net");
        _blobServiceClient = new BlobServiceClient(serviceUri, new DefaultAzureCredential());
    }

    /// <summary>
    /// Constructor for testing with custom BlobServiceClient.
    /// </summary>
    public DataLakeStorageService(
        DataLakeOptions options,
        BlobServiceClient blobServiceClient,
        ILogger<DataLakeStorageService> logger)
    {
        _options = options;
        _blobServiceClient = blobServiceClient;
        _logger = logger;
    }

    public async Task StoreBronzeAsync(
        TelemetryReading reading,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug(
                "Storing telemetry {TelemetryId} to bronze layer",
                reading.Id);

            // Bronze: raw/{yyyy}/{MM}/{dd}/{HH}/{deviceId}_{timestamp}.json
            var blobPath = GetBronzePath(reading);

            var containerClient = _blobServiceClient.GetBlobContainerClient(_options.BronzeContainer);
            await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            var blobClient = containerClient.GetBlockBlobClient(blobPath);

            // Serialize to JSON
            var json = SerializeToJson(reading);
            var content = Encoding.UTF8.GetBytes(json);

            // Upload with metadata
            var metadata = new Dictionary<string, string>
            {
                ["deviceId"] = reading.DeviceId.Value,
                ["timestamp"] = reading.Timestamp.Value.ToString("O"),
                ["isValid"] = reading.IsValid.ToString()
            };

            using var stream = new MemoryStream(content);
            var options = new BlobUploadOptions { Metadata = metadata };
            await blobClient.UploadAsync(stream, options, cancellationToken);

            _logger.LogDebug(
                "Stored telemetry {TelemetryId} to bronze: {BlobPath}",
                reading.Id,
                blobPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error storing telemetry {TelemetryId} to bronze layer",
                reading.Id);
            throw;
        }
    }

    public async Task StoreSilverAsync(
        TelemetryReading reading,
        Dictionary<string, string> enrichedMetadata,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug(
                "Storing telemetry {TelemetryId} to silver layer",
                reading.Id);

            // Silver: silver/{yyyy}/{MM}/{dd}/{HH}/{deviceId}_{timestamp}.json
            var blobPath = GetSilverPath(reading);

            var containerClient = _blobServiceClient.GetBlobContainerClient(_options.SilverContainer);
            await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            var blobClient = containerClient.GetBlockBlobClient(blobPath);

            // Serialize with enriched metadata
            var json = SerializeToJsonWithMetadata(reading, enrichedMetadata);
            var content = Encoding.UTF8.GetBytes(json);

            // Upload with metadata
            var metadata = new Dictionary<string, string>
            {
                ["deviceId"] = reading.DeviceId.Value,
                ["timestamp"] = reading.Timestamp.Value.ToString("O"),
                ["enriched"] = "true"
            };

            using var stream = new MemoryStream(content);
            var options = new BlobUploadOptions { Metadata = metadata };
            await blobClient.UploadAsync(stream, options, cancellationToken);

            _logger.LogDebug(
                "Stored telemetry {TelemetryId} to silver: {BlobPath}",
                reading.Id,
                blobPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error storing telemetry {TelemetryId} to silver layer",
                reading.Id);
            throw;
        }
    }

    public async Task StoreGoldAsync(
        Dictionary<string, object> aggregates,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Storing aggregates to gold layer");

            // Gold: gold/{yyyy}/{MM}/{dd}/hourly_aggregates_{HH}.json
            var timestamp = DateTimeOffset.UtcNow;
            var blobPath = $"gold/{timestamp:yyyy}/{timestamp:MM}/{timestamp:dd}/hourly_aggregates_{timestamp:HH}.json";

            var containerClient = _blobServiceClient.GetBlobContainerClient(_options.GoldContainer);
            await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            var blobClient = containerClient.GetBlockBlobClient(blobPath);

            // Serialize aggregates
            var json = JsonSerializer.Serialize(aggregates, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            var content = Encoding.UTF8.GetBytes(json);

            // Upload (replace if exists for hourly aggregates)
            using var stream = new MemoryStream(content);
            var options = new BlobUploadOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    ["timestamp"] = timestamp.ToString("O")
                }
            };
            await blobClient.UploadAsync(stream, options, cancellationToken);

            _logger.LogDebug("Stored aggregates to gold: {BlobPath}", blobPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing aggregates to gold layer");
            throw;
        }
    }

    private static string GetBronzePath(TelemetryReading reading)
    {
        var ts = reading.Timestamp.Value;
        return $"bronze/{ts:yyyy}/{ts:MM}/{ts:dd}/{ts:HH}/{reading.DeviceId.Value}_{ts:yyyyMMddHHmmss}.json";
    }

    private static string GetSilverPath(TelemetryReading reading)
    {
        var ts = reading.Timestamp.Value;
        return $"silver/{ts:yyyy}/{ts:MM}/{ts:dd}/{ts:HH}/{reading.DeviceId.Value}_{ts:yyyyMMddHHmmss}.json";
    }

    private static string SerializeToJson(TelemetryReading reading)
    {
        var data = new
        {
            id = reading.Id,
            deviceId = reading.DeviceId.Value,
            timestamp = reading.Timestamp.Value,
            receivedAt = reading.ReceivedAt.Value,
            isValid = reading.IsValid,
            validationError = reading.ValidationError,
            measurements = reading.Measurements.ToDictionary(
                kvp => kvp.Key,
                kvp => new
                {
                    value = kvp.Value.Value,
                    unit = kvp.Value.Unit,
                    quality = kvp.Value.Quality.ToString()
                })
        };

        return JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private static string SerializeToJsonWithMetadata(
        TelemetryReading reading,
        Dictionary<string, string> enrichedMetadata)
    {
        var data = new
        {
            id = reading.Id,
            deviceId = reading.DeviceId.Value,
            timestamp = reading.Timestamp.Value,
            receivedAt = reading.ReceivedAt.Value,
            isValid = reading.IsValid,
            validationError = reading.ValidationError,
            measurements = reading.Measurements.ToDictionary(
                kvp => kvp.Key,
                kvp => new
                {
                    value = kvp.Value.Value,
                    unit = kvp.Value.Unit,
                    quality = kvp.Value.Quality.ToString()
                }),
            metadata = enrichedMetadata
        };

        return JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}
