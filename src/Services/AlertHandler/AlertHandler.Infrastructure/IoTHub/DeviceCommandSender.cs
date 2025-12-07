using System.Text;
using System.Text.Json;
using AlertHandler.Application.Ports;
using Azure.Identity;
using IoTTelemetry.Domain.ValueObjects;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AlertHandler.Infrastructure.IoTHub;

/// <summary>
/// Sends Cloud-to-Device commands via Azure IoT Hub.
/// </summary>
public sealed class DeviceCommandSender : IDeviceCommandSender, IAsyncDisposable
{
    private readonly ServiceClient _serviceClient;
    private readonly IoTHubOptions _options;
    private readonly ILogger<DeviceCommandSender> _logger;

    public DeviceCommandSender(
        IOptions<IoTHubOptions> options,
        ILogger<DeviceCommandSender> logger)
    {
        _options = options.Value;
        _logger = logger;

        // Create Service Client with managed identity
        _serviceClient = ServiceClient.Create(
            _options.Hostname,
            new DefaultAzureCredential());
    }

    public async Task<bool> SendCommandAsync(
        DeviceId deviceId,
        string commandName,
        Dictionary<string, object> payload,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Sending C2D command '{CommandName}' to device {DeviceId}",
                commandName,
                deviceId);

            // Build command message
            var commandPayload = new
            {
                command = commandName,
                payload = payload,
                timestamp = DateTimeOffset.UtcNow
            };

            var messageBody = JsonSerializer.Serialize(commandPayload);
            var message = new Message(Encoding.UTF8.GetBytes(messageBody))
            {
                MessageId = Guid.NewGuid().ToString(),
                Ack = DeliveryAcknowledgement.Full,
                ExpiryTimeUtc = DateTime.UtcNow.AddSeconds(_options.DefaultMessageTtlSeconds)
            };

            // Add properties
            message.Properties["command"] = commandName;
            message.Properties["timestamp"] = DateTimeOffset.UtcNow.ToString("O");

            // Send command with timeout
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(_options.CommandTimeoutSeconds));

            await _serviceClient.SendAsync(deviceId.Value, message, cts.Token);

            _logger.LogInformation(
                "C2D command '{CommandName}' sent successfully to device {DeviceId}",
                commandName,
                deviceId);

            return true;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "C2D command '{CommandName}' to device {DeviceId} timed out",
                commandName,
                deviceId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send C2D command '{CommandName}' to device {DeviceId}",
                commandName,
                deviceId);
            return false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _serviceClient.CloseAsync();
        _serviceClient.Dispose();
    }
}
