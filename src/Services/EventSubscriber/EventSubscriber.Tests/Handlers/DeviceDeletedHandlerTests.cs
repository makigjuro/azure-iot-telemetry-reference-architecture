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

public sealed class DeviceDeletedHandlerTests
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly ILogger<DeviceDeletedHandler> _logger;
    private readonly DeviceDeletedHandler _handler;

    public DeviceDeletedHandlerTests()
    {
        _deviceRepository = Substitute.For<IDeviceRepository>();
        _logger = Substitute.For<ILogger<DeviceDeletedHandler>>();
        _handler = new DeviceDeletedHandler(_deviceRepository, _logger);
    }

    [Fact]
    public async Task Handle_WithExistingDevice_MarksAsDecommissionedAndReturnsSyncCommand()
    {
        // Arrange
        var deviceId = "device-001";
        var device = Device.Create(
            DeviceId.Create(deviceId),
            "Test Device",
            "Sensor");

        var command = new DeviceDeletedCommand(
            deviceId,
            "Microsoft.Devices.DeviceDeleted",
            DateTimeOffset.UtcNow);

        _deviceRepository.GetDeviceByIdAsync(Arg.Any<DeviceId>(), Arg.Any<CancellationToken>())
            .Returns(device);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.DeviceId.Value.Should().Be(deviceId);
        result.Operation.Should().Be(SyncOperation.Delete);

        device.Status.Should().Be(DeviceStatus.Decommissioned);

        await _deviceRepository.Received(1).SaveDeviceAsync(
            Arg.Is<Device>(d => d.Status == DeviceStatus.Decommissioned),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentDevice_ReturnsSyncCommandForCleanup()
    {
        // Arrange
        var deviceId = "device-002";
        var command = new DeviceDeletedCommand(
            deviceId,
            "Microsoft.Devices.DeviceDeleted",
            DateTimeOffset.UtcNow);

        _deviceRepository.GetDeviceByIdAsync(Arg.Any<DeviceId>(), Arg.Any<CancellationToken>())
            .Returns((Device?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.DeviceId.Value.Should().Be(deviceId);
        result.Operation.Should().Be(SyncOperation.Delete);

        await _deviceRepository.DidNotReceive().SaveDeviceAsync(
            Arg.Any<Device>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_PropagatesException()
    {
        // Arrange
        var deviceId = "device-003";
        var device = Device.Create(
            DeviceId.Create(deviceId),
            "Test Device",
            "Sensor");

        var command = new DeviceDeletedCommand(
            deviceId,
            "Microsoft.Devices.DeviceDeleted",
            DateTimeOffset.UtcNow);

        _deviceRepository.GetDeviceByIdAsync(Arg.Any<DeviceId>(), Arg.Any<CancellationToken>())
            .Returns(device);

        _deviceRepository.SaveDeviceAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Database error"));

        // Act & Assert
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }

    [Fact]
    public async Task Handle_WithActiveDevice_TransitionsToDecommissioned()
    {
        // Arrange
        var deviceId = "device-004";
        var device = Device.Create(
            DeviceId.Create(deviceId),
            "Active Device",
            "Sensor");
        device.Activate();

        var command = new DeviceDeletedCommand(
            deviceId,
            "Microsoft.Devices.DeviceDeleted",
            DateTimeOffset.UtcNow);

        _deviceRepository.GetDeviceByIdAsync(Arg.Any<DeviceId>(), Arg.Any<CancellationToken>())
            .Returns(device);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        device.Status.Should().Be(DeviceStatus.Decommissioned);
        device.LastModifiedAt.Should().NotBeNull();

        await _deviceRepository.Received(1).SaveDeviceAsync(
            Arg.Is<Device>(d =>
                d.Id.Value == deviceId &&
                d.Status == DeviceStatus.Decommissioned),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AlwaysReturnsSyncDeleteCommand()
    {
        // Arrange
        var deviceId = "device-005";
        var command = new DeviceDeletedCommand(
            deviceId,
            "Microsoft.Devices.DeviceDeleted",
            DateTimeOffset.UtcNow);

        _deviceRepository.GetDeviceByIdAsync(Arg.Any<DeviceId>(), Arg.Any<CancellationToken>())
            .Returns((Device?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Operation.Should().Be(SyncOperation.Delete);
    }
}
