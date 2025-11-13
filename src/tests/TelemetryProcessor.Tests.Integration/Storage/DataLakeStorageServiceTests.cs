using System.Text;
using System.Text.Json;
using Azure.Storage.Blobs;
using FluentAssertions;
using IoTTelemetry.Domain.Entities;
using IoTTelemetry.Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using TelemetryProcessor.Infrastructure.Storage;
using TelemetryProcessor.Tests.Integration.Fixtures;

namespace TelemetryProcessor.Tests.Integration.Storage;

/// <summary>
/// Integration tests for DataLakeStorageService using Azurite Testcontainer.
/// </summary>
public sealed class DataLakeStorageServiceTests : IClassFixture<AzuriteTestFixture>
{
    private readonly AzuriteTestFixture _fixture;

    public DataLakeStorageServiceTests(AzuriteTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task StoreBronzeAsync_StoresTelemetryInCorrectPath()
    {
        // Arrange
        var options = new DataLakeOptions
        {
            AccountName = "devstoreaccount1",
            BronzeContainer = "bronze",
            SilverContainer = "silver",
            GoldContainer = "gold"
        };

        var blobServiceClient = _fixture.CreateBlobServiceClient();
        var service = new DataLakeStorageService(options, blobServiceClient, NullLogger<DataLakeStorageService>.Instance);

        var deviceId = DeviceId.Create("test-device-001");
        var timestamp = new DateTimeOffset(2025, 11, 13, 14, 30, 0, TimeSpan.Zero);
        var reading = TelemetryReading.Create(
            deviceId,
            Timestamp.Create(timestamp),
            new Dictionary<string, TelemetryValue>
            {
                ["temperature"] = TelemetryValue.Create(23.5, "°C", TelemetryQuality.Good)
            }
        );

        // Act
        await service.StoreBronzeAsync(reading);

        // Assert
        var containerClient = blobServiceClient.GetBlobContainerClient("bronze");
        var exists = await containerClient.ExistsAsync();
        exists.Value.Should().BeTrue();

        var expectedPath = $"bronze/2025/11/13/14/{deviceId.Value}_20251113143000.json";
        var blobClient = containerClient.GetBlobClient(expectedPath);
        var blobExists = await blobClient.ExistsAsync();
        blobExists.Value.Should().BeTrue();

        // Verify content
        var download = await blobClient.DownloadContentAsync();
        var content = download.Value.Content.ToString();
        var json = JsonDocument.Parse(content);

        json.RootElement.GetProperty("deviceId").GetString().Should().Be(deviceId.Value);
        json.RootElement.GetProperty("isValid").GetBoolean().Should().BeTrue();
        json.RootElement.GetProperty("measurements").GetProperty("temperature").GetProperty("value").GetDouble().Should().Be(23.5);
        json.RootElement.GetProperty("measurements").GetProperty("temperature").GetProperty("unit").GetString().Should().Be("°C");

        // Verify metadata
        var properties = await blobClient.GetPropertiesAsync();
        properties.Value.Metadata.Should().ContainKey("deviceId").WhoseValue.Should().Be(deviceId.Value);
        properties.Value.Metadata.Should().ContainKey("timestamp");
        properties.Value.Metadata.Should().ContainKey("isValid").WhoseValue.Should().Be("True");
    }

    [Fact]
    public async Task StoreSilverAsync_StoresEnrichedTelemetryWithMetadata()
    {
        // Arrange
        var options = new DataLakeOptions
        {
            AccountName = "devstoreaccount1",
            BronzeContainer = "bronze",
            SilverContainer = "silver",
            GoldContainer = "gold"
        };

        var blobServiceClient = _fixture.CreateBlobServiceClient();
        var service = new DataLakeStorageService(options, blobServiceClient, NullLogger<DataLakeStorageService>.Instance);

        var deviceId = DeviceId.Create("test-device-002");
        var timestamp = new DateTimeOffset(2025, 11, 13, 15, 45, 0, TimeSpan.Zero);
        var reading = TelemetryReading.Create(
            deviceId,
            Timestamp.Create(timestamp),
            new Dictionary<string, TelemetryValue>
            {
                ["humidity"] = TelemetryValue.Create(65.0, "%", TelemetryQuality.Good)
            }
        );

        var enrichedMetadata = new Dictionary<string, string>
        {
            ["deviceName"] = "Sensor 002",
            ["deviceType"] = "HumiditySensor",
            ["location"] = "Building A"
        };

        // Act
        await service.StoreSilverAsync(reading, enrichedMetadata);

        // Assert
        var containerClient = blobServiceClient.GetBlobContainerClient("silver");
        var exists = await containerClient.ExistsAsync();
        exists.Value.Should().BeTrue();

        var expectedPath = $"silver/2025/11/13/15/{deviceId.Value}_20251113154500.json";
        var blobClient = containerClient.GetBlobClient(expectedPath);
        var blobExists = await blobClient.ExistsAsync();
        blobExists.Value.Should().BeTrue();

        // Verify content includes enriched metadata
        var download = await blobClient.DownloadContentAsync();
        var content = download.Value.Content.ToString();
        var json = JsonDocument.Parse(content);

        json.RootElement.GetProperty("deviceId").GetString().Should().Be(deviceId.Value);
        json.RootElement.GetProperty("metadata").GetProperty("deviceName").GetString().Should().Be("Sensor 002");
        json.RootElement.GetProperty("metadata").GetProperty("deviceType").GetString().Should().Be("HumiditySensor");
        json.RootElement.GetProperty("metadata").GetProperty("location").GetString().Should().Be("Building A");

        // Verify blob metadata
        var properties = await blobClient.GetPropertiesAsync();
        properties.Value.Metadata.Should().ContainKey("enriched").WhoseValue.Should().Be("true");
    }

    [Fact]
    public async Task StoreGoldAsync_StoresAggregatesInHourlyPath()
    {
        // Arrange
        var options = new DataLakeOptions
        {
            AccountName = "devstoreaccount1",
            BronzeContainer = "bronze",
            SilverContainer = "silver",
            GoldContainer = "gold"
        };

        var blobServiceClient = _fixture.CreateBlobServiceClient();
        var service = new DataLakeStorageService(options, blobServiceClient, NullLogger<DataLakeStorageService>.Instance);

        var aggregates = new Dictionary<string, object>
        {
            ["totalReadings"] = 150,
            ["averageTemperature"] = 22.5,
            ["maxTemperature"] = 28.0,
            ["minTemperature"] = 18.5,
            ["deviceCount"] = 10
        };

        // Act
        await service.StoreGoldAsync(aggregates);

        // Assert
        var containerClient = blobServiceClient.GetBlobContainerClient("gold");
        var exists = await containerClient.ExistsAsync();
        exists.Value.Should().BeTrue();

        // Find the blob (path includes current timestamp)
        var blobs = containerClient.GetBlobsAsync(prefix: "gold/");
        var blobList = new List<string>();
        await foreach (var blob in blobs)
        {
            blobList.Add(blob.Name);
        }

        blobList.Should().ContainSingle();
        var blobName = blobList[0];
        blobName.Should().MatchRegex(@"gold/\d{4}/\d{2}/\d{2}/hourly_aggregates_\d{2}\.json");

        // Verify content
        var blobClient = containerClient.GetBlobClient(blobName);
        var download = await blobClient.DownloadContentAsync();
        var content = download.Value.Content.ToString();
        var json = JsonDocument.Parse(content);

        json.RootElement.GetProperty("totalReadings").GetInt32().Should().Be(150);
        json.RootElement.GetProperty("averageTemperature").GetDouble().Should().Be(22.5);
        json.RootElement.GetProperty("deviceCount").GetInt32().Should().Be(10);
    }

    [Fact]
    public async Task StoreBronzeAsync_WithInvalidReading_StoresWithIsValidFalse()
    {
        // Arrange
        var options = new DataLakeOptions
        {
            AccountName = "devstoreaccount1",
            BronzeContainer = "bronze",
            SilverContainer = "silver",
            GoldContainer = "gold"
        };

        var blobServiceClient = _fixture.CreateBlobServiceClient();
        var service = new DataLakeStorageService(options, blobServiceClient, NullLogger<DataLakeStorageService>.Instance);

        var deviceId = DeviceId.Create("test-device-003");
        var timestamp = new DateTimeOffset(2025, 11, 13, 16, 0, 0, TimeSpan.Zero);
        var reading = TelemetryReading.Create(
            deviceId,
            Timestamp.Create(timestamp),
            new Dictionary<string, TelemetryValue>()  // Empty measurements
        );

        reading.MarkAsInvalid("No measurements provided");

        // Act
        await service.StoreBronzeAsync(reading);

        // Assert
        var containerClient = blobServiceClient.GetBlobContainerClient("bronze");
        var expectedPath = $"bronze/2025/11/13/16/{deviceId.Value}_20251113160000.json";
        var blobClient = containerClient.GetBlobClient(expectedPath);

        var download = await blobClient.DownloadContentAsync();
        var content = download.Value.Content.ToString();
        var json = JsonDocument.Parse(content);

        json.RootElement.GetProperty("isValid").GetBoolean().Should().BeFalse();
        json.RootElement.GetProperty("validationError").GetString().Should().Be("No measurements provided");

        // Verify metadata
        var properties = await blobClient.GetPropertiesAsync();
        properties.Value.Metadata["isValid"].Should().Be("False");
    }

    [Fact]
    public async Task StoreAsync_WithCancellationToken_RespectsToken()
    {
        // Arrange
        var options = new DataLakeOptions
        {
            AccountName = "devstoreaccount1",
            BronzeContainer = "bronze"
        };

        var blobServiceClient = _fixture.CreateBlobServiceClient();
        var service = new DataLakeStorageService(options, blobServiceClient, NullLogger<DataLakeStorageService>.Instance);

        var deviceId = DeviceId.Create("test-device-004");
        var reading = TelemetryReading.Create(
            deviceId,
            Timestamp.Now(),
            new Dictionary<string, TelemetryValue>
            {
                ["temp"] = TelemetryValue.Create(20.0, "°C", TelemetryQuality.Good)
            }
        );

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        Func<Task> act = async () => await service.StoreBronzeAsync(reading, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
