using System.Text.Json;
using Azure;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using EventSubscriber.Application.Ports;
using IoTTelemetry.Domain.Entities;
using IoTTelemetry.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventSubscriber.Infrastructure.DigitalTwins;

/// <summary>
/// Azure Digital Twins service implementation.
/// Manages device digital twins lifecycle.
/// </summary>
public sealed class DigitalTwinService : IDigitalTwinService, IAsyncDisposable
{
    private readonly DigitalTwinsClient _client;
    private readonly DigitalTwinOptions _options;
    private readonly ILogger<DigitalTwinService> _logger;

    public DigitalTwinService(
        IOptions<DigitalTwinOptions> options,
        ILogger<DigitalTwinService> logger)
    {
        _options = options.Value;
        _logger = logger;

        _client = new DigitalTwinsClient(
            new Uri(_options.InstanceUrl),
            new DefaultAzureCredential());
    }

    public async Task<bool> CreateOrUpdateTwinAsync(
        Device device,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Creating/updating digital twin for device {DeviceId}",
                device.Id);

            var twinId = device.Id.Value;

            // Build digital twin data
            var twinData = new
            {
                metadata = new
                {
                    modelId = _options.DeviceModelId
                },
                name = device.Name,
                type = device.Type,
                status = device.Status.ToString(),
                location = device.Location,
                createdAt = device.CreatedAt.Value,
                lastModifiedAt = device.LastModifiedAt?.Value,
                lastSeenAt = device.LastSeenAt?.Value,
                properties = device.Properties
            };

            var twinJson = JsonSerializer.Serialize(twinData);

            // Check if twin exists
            var exists = await TwinExistsAsync(device.Id, cancellationToken);

            if (exists)
            {
                // Update existing twin
                var updateDocument = new JsonPatchDocument();
                updateDocument.AppendReplace("/name", device.Name);
                updateDocument.AppendReplace("/type", device.Type);
                updateDocument.AppendReplace("/status", device.Status.ToString());

                if (device.Location is not null)
                {
                    updateDocument.AppendReplace("/location", device.Location);
                }

                if (device.LastModifiedAt is not null)
                {
                    updateDocument.AppendReplace("/lastModifiedAt", device.LastModifiedAt.Value);
                }

                if (device.LastSeenAt is not null)
                {
                    updateDocument.AppendReplace("/lastSeenAt", device.LastSeenAt.Value);
                }

                updateDocument.AppendReplace("/properties", device.Properties);

                await _client.UpdateDigitalTwinAsync(
                    twinId,
                    updateDocument,
                    cancellationToken: cancellationToken);

                _logger.LogInformation(
                    "Digital twin updated for device {DeviceId}",
                    device.Id);
            }
            else
            {
                // Create new twin
                await _client.CreateOrReplaceDigitalTwinAsync(
                    twinId,
                    twinJson,
                    cancellationToken: CancellationToken.None);

                _logger.LogInformation(
                    "Digital twin created for device {DeviceId}",
                    device.Id);
            }

            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogError(
                ex,
                "Digital Twins model not found for device {DeviceId}. Ensure model {ModelId} is uploaded.",
                device.Id,
                _options.DeviceModelId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create/update digital twin for device {DeviceId}",
                device.Id);
            return false;
        }
    }

    public async Task<bool> DeleteTwinAsync(
        DeviceId deviceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Deleting digital twin for device {DeviceId}",
                deviceId);

            await _client.DeleteDigitalTwinAsync(
                deviceId.Value,
                cancellationToken: CancellationToken.None);

            _logger.LogInformation(
                "Digital twin deleted for device {DeviceId}",
                deviceId);

            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning(
                ex,
                "Digital twin not found for device {DeviceId}. May have already been deleted.",
                deviceId);
            return true; // Consider missing twin as successful deletion
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to delete digital twin for device {DeviceId}",
                deviceId);
            return false;
        }
    }

    public async Task<bool> TwinExistsAsync(
        DeviceId deviceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.GetDigitalTwinAsync<BasicDigitalTwin>(
                deviceId.Value,
                cancellationToken);
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        // DigitalTwinsClient doesn't implement IDisposable in current SDK
        await Task.CompletedTask;
    }
}
