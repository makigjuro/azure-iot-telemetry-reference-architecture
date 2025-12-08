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

public sealed class SyncDigitalTwinHandlerTests
{
    private readonly IDigitalTwinService _digitalTwinService;
    private readonly IDeviceRepository _deviceRepository;
    private readonly ILogger<SyncDigitalTwinHandler> _logger;
    private readonly SyncDigitalTwinHandler _handler;

    public SyncDigitalTwinHandlerTests()
    {
        _digitalTwinService = Substitute.For<IDigitalTwinService>();
        _deviceRepository = Substitute.For<IDeviceRepository>();
        _logger = Substitute.For<ILogger<SyncDigitalTwinHandler>>();
        _handler = new SyncDigitalTwinHandler(_digitalTwinService, _deviceRepository, _logger);
    }

    [Fact]
    public async Task Handle_WithCreateOperation_CreatesDigitalTwin()
    {
        // Arrange
        var deviceId = DeviceId.Create("device-001");
        var device = Device.Create(deviceId, "Test Device", "Sensor");

        var command = new SyncDigitalTwinCommand(
            deviceId,
            SyncOperation.CreateOrUpdate,
            new Dictionary<string, object>());

        _deviceRepository.GetDeviceByIdAsync(deviceId, Arg.Any<CancellationToken>())
            .Returns(device);

        _digitalTwinService.CreateOrUpdateTwinAsync(device, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _digitalTwinService.Received(1).CreateOrUpdateTwinAsync(
            Arg.Is<Device>(d => d.Id == deviceId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDeleteOperation_DeletesDigitalTwin()
    {
        // Arrange
        var deviceId = DeviceId.Create("device-002");
        var command = new SyncDigitalTwinCommand(
            deviceId,
            SyncOperation.Delete);

        _digitalTwinService.DeleteTwinAsync(deviceId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _digitalTwinService.Received(1).DeleteTwinAsync(
            deviceId,
            Arg.Any<CancellationToken>());

        await _deviceRepository.DidNotReceive().GetDeviceByIdAsync(
            Arg.Any<DeviceId>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithCreateOperationButDeviceNotFound_LogsWarningAndReturns()
    {
        // Arrange
        var deviceId = DeviceId.Create("device-003");
        var command = new SyncDigitalTwinCommand(
            deviceId,
            SyncOperation.CreateOrUpdate);

        _deviceRepository.GetDeviceByIdAsync(deviceId, Arg.Any<CancellationToken>())
            .Returns((Device?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _digitalTwinService.DidNotReceive().CreateOrUpdateTwinAsync(
            Arg.Any<Device>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCreateFails_ThrowsException()
    {
        // Arrange
        var deviceId = DeviceId.Create("device-004");
        var device = Device.Create(deviceId, "Test Device", "Sensor");

        var command = new SyncDigitalTwinCommand(
            deviceId,
            SyncOperation.CreateOrUpdate);

        _deviceRepository.GetDeviceByIdAsync(deviceId, Arg.Any<CancellationToken>())
            .Returns(device);

        _digitalTwinService.CreateOrUpdateTwinAsync(device, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act & Assert
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Failed to sync digital twin for device {deviceId}");
    }

    [Fact]
    public async Task Handle_WhenDeleteFails_LogsWarning()
    {
        // Arrange
        var deviceId = DeviceId.Create("device-005");
        var command = new SyncDigitalTwinCommand(
            deviceId,
            SyncOperation.Delete);

        _digitalTwinService.DeleteTwinAsync(deviceId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - should not throw
        await _digitalTwinService.Received(1).DeleteTwinAsync(
            deviceId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDigitalTwinServiceThrows_PropagatesException()
    {
        // Arrange
        var deviceId = DeviceId.Create("device-006");
        var device = Device.Create(deviceId, "Test Device", "Sensor");

        var command = new SyncDigitalTwinCommand(
            deviceId,
            SyncOperation.CreateOrUpdate);

        _deviceRepository.GetDeviceByIdAsync(deviceId, Arg.Any<CancellationToken>())
            .Returns(device);

        _digitalTwinService.CreateOrUpdateTwinAsync(device, Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Digital Twins error"));

        // Act & Assert
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Digital Twins error");
    }

    [Fact]
    public async Task Handle_WithDeleteOperationAndSuccess_LogsSuccess()
    {
        // Arrange
        var deviceId = DeviceId.Create("device-007");
        var command = new SyncDigitalTwinCommand(
            deviceId,
            SyncOperation.Delete);

        _digitalTwinService.DeleteTwinAsync(deviceId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _digitalTwinService.Received(1).DeleteTwinAsync(
            deviceId,
            Arg.Any<CancellationToken>());
    }
}
