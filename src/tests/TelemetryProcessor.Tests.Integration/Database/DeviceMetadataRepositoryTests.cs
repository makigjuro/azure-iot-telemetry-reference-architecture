using FluentAssertions;
using IoTTelemetry.Domain.Entities;
using IoTTelemetry.Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using TelemetryProcessor.Infrastructure.Database;
using TelemetryProcessor.Tests.Integration.Fixtures;

namespace TelemetryProcessor.Tests.Integration.Database;

/// <summary>
/// Integration tests for DeviceMetadataRepository using Testcontainers.
/// </summary>
public sealed class DeviceMetadataRepositoryTests : IClassFixture<PostgreSqlTestFixture>
{
    private readonly PostgreSqlTestFixture _fixture;

    public DeviceMetadataRepositoryTests(PostgreSqlTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetMetadataAsync_WhenDeviceExists_ReturnsMetadataDictionary()
    {
        // Arrange
        var deviceId = DeviceId.Create("test-device-001");
        var device = Device.Create(deviceId, "Test Temperature Sensor", "TemperatureSensor");
        device.UpdateLocation("Building A - Room 101");
        device.SetProperty("manufacturer", "Acme Corp");
        device.SetProperty("model", "TS-2000");
        device.SetProperty("firmwareVersion", "1.2.3");
        device.Activate(); // Set status to Active

        await using var context = _fixture.CreateDbContext();
        context.Devices.Add(device);
        await context.SaveChangesAsync();

        var repository = new DeviceMetadataRepository(context, NullLogger<DeviceMetadataRepository>.Instance);

        // Act
        var metadata = await repository.GetMetadataAsync(deviceId);

        // Assert
        metadata.Should().NotBeNull();
        metadata.Should().ContainKey("deviceName").WhoseValue.Should().Be("Test Temperature Sensor");
        metadata.Should().ContainKey("deviceType").WhoseValue.Should().Be("TemperatureSensor");
        metadata.Should().ContainKey("deviceStatus").WhoseValue.Should().Be(DeviceStatus.Active.ToString());
        metadata.Should().ContainKey("location").WhoseValue.Should().Be("Building A - Room 101");
        metadata.Should().ContainKey("manufacturer").WhoseValue.Should().Be("Acme Corp");
        metadata.Should().ContainKey("model").WhoseValue.Should().Be("TS-2000");
        metadata.Should().ContainKey("firmwareVersion").WhoseValue.Should().Be("1.2.3");
    }

    [Fact]
    public async Task GetMetadataAsync_WhenDeviceDoesNotExist_ReturnsNull()
    {
        // Arrange
        var deviceId = DeviceId.Create("non-existent-device");
        await using var context = _fixture.CreateDbContext();
        var repository = new DeviceMetadataRepository(context, NullLogger<DeviceMetadataRepository>.Instance);

        // Act
        var metadata = await repository.GetMetadataAsync(deviceId);

        // Assert
        metadata.Should().BeNull();
    }

    [Fact]
    public async Task GetMetadataAsync_WhenDeviceHasNoLocation_ExcludesLocationFromMetadata()
    {
        // Arrange
        var deviceId = DeviceId.Create("test-device-002");
        var device = Device.Create(deviceId, "Mobile Sensor", "HumiditySensor");
        // No location set

        await using var context = _fixture.CreateDbContext();
        context.Devices.Add(device);
        await context.SaveChangesAsync();

        var repository = new DeviceMetadataRepository(context, NullLogger<DeviceMetadataRepository>.Instance);

        // Act
        var metadata = await repository.GetMetadataAsync(deviceId);

        // Assert
        metadata.Should().NotBeNull();
        metadata.Should().NotContainKey("location");
        metadata.Should().ContainKey("deviceName");
        metadata.Should().ContainKey("deviceType");
        metadata.Should().ContainKey("deviceStatus");
    }

    [Fact]
    public async Task GetMetadataAsync_WithMultipleDevices_ReturnsCorrectDevice()
    {
        // Arrange
        var deviceId1 = DeviceId.Create("test-device-003");
        var deviceId2 = DeviceId.Create("test-device-004");

        var device1 = Device.Create(deviceId1, "Device 1", "Type A");
        device1.UpdateLocation("Location A");

        var device2 = Device.Create(deviceId2, "Device 2", "Type B");
        device2.UpdateLocation("Location B");

        await using var context = _fixture.CreateDbContext();
        context.Devices.AddRange(device1, device2);
        await context.SaveChangesAsync();

        var repository = new DeviceMetadataRepository(context, NullLogger<DeviceMetadataRepository>.Instance);

        // Act
        var metadata1 = await repository.GetMetadataAsync(deviceId1);
        var metadata2 = await repository.GetMetadataAsync(deviceId2);

        // Assert
        metadata1.Should().NotBeNull();
        metadata1!["deviceName"].Should().Be("Device 1");
        metadata1["deviceType"].Should().Be("Type A");
        metadata1["location"].Should().Be("Location A");

        metadata2.Should().NotBeNull();
        metadata2!["deviceName"].Should().Be("Device 2");
        metadata2["deviceType"].Should().Be("Type B");
        metadata2["location"].Should().Be("Location B");
    }

    [Fact]
    public async Task GetMetadataAsync_WithCancellationToken_RespectsToken()
    {
        // Arrange
        var deviceId = DeviceId.Create("test-device-005");
        await using var context = _fixture.CreateDbContext();
        var repository = new DeviceMetadataRepository(context, NullLogger<DeviceMetadataRepository>.Instance);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        Func<Task> act = async () => await repository.GetMetadataAsync(deviceId, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
