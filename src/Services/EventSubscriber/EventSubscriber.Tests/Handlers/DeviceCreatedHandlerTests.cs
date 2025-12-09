using EventSubscriber.Application.Commands;
using EventSubscriber.Application.Handlers;
using EventSubscriber.Application.Ports;
using FluentAssertions;
using IoTTelemetry.Domain.Entities;
using IoTTelemetry.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace EventSubscriber.Tests.Handlers;

public sealed class DeviceCreatedHandlerTests
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly ILogger<DeviceCreatedHandler> _logger;
    private readonly DeviceCreatedHandler _handler;

    public DeviceCreatedHandlerTests()
    {
        _deviceRepository = Substitute.For<IDeviceRepository>();
        _logger = Substitute.For<ILogger<DeviceCreatedHandler>>();
        _handler = new DeviceCreatedHandler(_deviceRepository, _logger);
    }

    [Fact]
    public async Task Handle_WithNewDevice_CreatesDeviceAndReturnsSyncCommand()
    {
        // Arrange
        var deviceId = "device-001";
        var command = new DeviceCreatedCommand(
            deviceId,
            "Microsoft.Devices.DeviceCreated",
            DateTimeOffset.UtcNow,
            new Dictionary<string, object>
            {
                ["deviceName"] = "Temperature Sensor 001",
                ["deviceType"] = "TemperatureSensor",
                ["location"] = "Building A - Floor 2"
            });

        _deviceRepository.GetDeviceByIdAsync(Arg.Any<DeviceId>(), Arg.Any<CancellationToken>())
            .Returns((Device?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.DeviceId.Value.Should().Be(deviceId);
        result.Operation.Should().Be(SyncOperation.CreateOrUpdate);
        result.Metadata.Should().NotBeNull();
        result.Metadata!["name"].Should().Be("Temperature Sensor 001");
        result.Metadata["type"].Should().Be("TemperatureSensor");
        result.Metadata["status"].Should().Be("Active");

        await _deviceRepository.Received(1).SaveDeviceAsync(
            Arg.Is<Device>(d => d.Id.Value == deviceId && d.Name == "Temperature Sensor 001"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithExistingDevice_SkipsCreationAndReturnsNull()
    {
        // Arrange
        var deviceId = "device-001";
        var existingDevice = Device.Create(
            DeviceId.Create(deviceId),
            "Existing Device",
            "Sensor");

        var command = new DeviceCreatedCommand(
            deviceId,
            "Microsoft.Devices.DeviceCreated",
            DateTimeOffset.UtcNow,
            new Dictionary<string, object>());

        _deviceRepository.GetDeviceByIdAsync(Arg.Any<DeviceId>(), Arg.Any<CancellationToken>())
            .Returns(existingDevice);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        await _deviceRepository.DidNotReceive().SaveDeviceAsync(
            Arg.Any<Device>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithMinimalData_CreatesDeviceWithDefaults()
    {
        // Arrange
        var deviceId = "device-002";
        var command = new DeviceCreatedCommand(
            deviceId,
            "Microsoft.Devices.DeviceCreated",
            DateTimeOffset.UtcNow,
            null);

        _deviceRepository.GetDeviceByIdAsync(Arg.Any<DeviceId>(), Arg.Any<CancellationToken>())
            .Returns((Device?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.DeviceId.Value.Should().Be(deviceId);

        await _deviceRepository.Received(1).SaveDeviceAsync(
            Arg.Is<Device>(d =>
                d.Id.Value == deviceId &&
                d.Name == deviceId &&
                d.Type == "Unknown"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithCustomProperties_SetsPropertiesOnDevice()
    {
        // Arrange
        var deviceId = "device-003";
        var command = new DeviceCreatedCommand(
            deviceId,
            "Microsoft.Devices.DeviceCreated",
            DateTimeOffset.UtcNow,
            new Dictionary<string, object>
            {
                ["deviceName"] = "Test Device",
                ["deviceType"] = "TestSensor",
                ["properties"] = new Dictionary<string, object>
                {
                    ["firmware"] = "v1.2.3",
                    ["manufacturer"] = "ACME Corp"
                }
            });

        _deviceRepository.GetDeviceByIdAsync(Arg.Any<DeviceId>(), Arg.Any<CancellationToken>())
            .Returns((Device?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        await _deviceRepository.Received(1).SaveDeviceAsync(
            Arg.Is<Device>(d =>
                d.Properties.ContainsKey("firmware") &&
                d.Properties.ContainsKey("manufacturer")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_PropagatesException()
    {
        // Arrange
        var deviceId = "device-004";
        var command = new DeviceCreatedCommand(
            deviceId,
            "Microsoft.Devices.DeviceCreated",
            DateTimeOffset.UtcNow,
            new Dictionary<string, object>());

        _deviceRepository.GetDeviceByIdAsync(Arg.Any<DeviceId>(), Arg.Any<CancellationToken>())
            .Returns((Device?)null);

        _deviceRepository.SaveDeviceAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Database error"));

        // Act & Assert
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }

    [Fact]
    public async Task Handle_WithLocation_SetsLocationOnDevice()
    {
        // Arrange
        var deviceId = "device-005";
        var location = "Warehouse B - Zone 3";
        var command = new DeviceCreatedCommand(
            deviceId,
            "Microsoft.Devices.DeviceCreated",
            DateTimeOffset.UtcNow,
            new Dictionary<string, object>
            {
                ["deviceName"] = "Device 005",
                ["deviceType"] = "Sensor",
                ["location"] = location
            });

        _deviceRepository.GetDeviceByIdAsync(Arg.Any<DeviceId>(), Arg.Any<CancellationToken>())
            .Returns((Device?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        await _deviceRepository.Received(1).SaveDeviceAsync(
            Arg.Is<Device>(d => d.Location == location),
            Arg.Any<CancellationToken>());
    }
}
