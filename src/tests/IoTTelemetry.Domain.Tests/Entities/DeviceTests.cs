using IoTTelemetry.Domain.Entities;
using IoTTelemetry.Domain.Events;
using IoTTelemetry.Domain.ValueObjects;

namespace IoTTelemetry.Domain.Tests.Entities;

public class DeviceTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateDevice()
    {
        // Arrange
        var deviceId = DeviceId.Create("device-001");
        const string name = "Temperature Sensor";
        const string type = "Sensor";

        // Act
        var device = Device.Create(deviceId, name, type);

        // Assert
        device.Id.Should().Be(deviceId);
        device.Name.Should().Be(name);
        device.Type.Should().Be(type);
        device.Status.Should().Be(DeviceStatus.Registered);
        device.CreatedAt.Should().NotBeNull();
        device.LastSeenAt.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldRaiseDeviceRegisteredEvent()
    {
        // Arrange
        var deviceId = DeviceId.Create("device-001");

        // Act
        var device = Device.Create(deviceId, "Test Device", "Sensor");

        // Assert
        device.DomainEvents.Should().HaveCount(1);
        device.DomainEvents.First().Should().BeOfType<DeviceRegisteredEvent>();
        var @event = (DeviceRegisteredEvent)device.DomainEvents.First();
        @event.DeviceId.Should().Be(deviceId);
        @event.Name.Should().Be("Test Device");
        @event.Type.Should().Be("Sensor");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        var deviceId = DeviceId.Create("device-001");

        // Act
        var act = () => Device.Create(deviceId, invalidName!, "Sensor");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Device name cannot be empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyType_ShouldThrowArgumentException(string invalidType)
    {
        // Arrange
        var deviceId = DeviceId.Create("device-001");

        // Act
        var act = () => Device.Create(deviceId, "Test Device", invalidType!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Device type cannot be empty*");
    }

    [Fact]
    public void Activate_WhenRegistered_ShouldChangeStatusToActive()
    {
        // Arrange
        var device = Device.Create(DeviceId.Create("device-001"), "Test Device", "Sensor");

        // Act
        device.Activate();

        // Assert
        device.Status.Should().Be(DeviceStatus.Active);
        device.LastModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldRemainActive()
    {
        // Arrange
        var device = Device.Create(DeviceId.Create("device-001"), "Test Device", "Sensor");
        device.Activate();
        var modifiedAt = device.LastModifiedAt;

        // Act
        device.Activate();

        // Assert
        device.Status.Should().Be(DeviceStatus.Active);
        device.LastModifiedAt.Should().Be(modifiedAt);
    }

    [Fact]
    public void Activate_WhenDecommissioned_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var device = Device.Create(DeviceId.Create("device-001"), "Test Device", "Sensor");
        device.Decommission();

        // Act
        var act = () => device.Activate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot activate a decommissioned device*");
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldChangeStatusToInactive()
    {
        // Arrange
        var device = Device.Create(DeviceId.Create("device-001"), "Test Device", "Sensor");
        device.Activate();

        // Act
        device.Deactivate();

        // Assert
        device.Status.Should().Be(DeviceStatus.Inactive);
        device.LastModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_WhenDecommissioned_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var device = Device.Create(DeviceId.Create("device-001"), "Test Device", "Sensor");
        device.Decommission();

        // Act
        var act = () => device.Deactivate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot deactivate a decommissioned device*");
    }

    [Fact]
    public void Disable_WhenActive_ShouldChangeStatusToDisabled()
    {
        // Arrange
        var device = Device.Create(DeviceId.Create("device-001"), "Test Device", "Sensor");
        device.Activate();

        // Act
        device.Disable();

        // Assert
        device.Status.Should().Be(DeviceStatus.Disabled);
        device.LastModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void StartMaintenance_WhenActive_ShouldChangeStatusToMaintenance()
    {
        // Arrange
        var device = Device.Create(DeviceId.Create("device-001"), "Test Device", "Sensor");
        device.Activate();

        // Act
        device.StartMaintenance();

        // Assert
        device.Status.Should().Be(DeviceStatus.Maintenance);
        device.LastModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void EndMaintenance_WhenInMaintenance_ShouldChangeStatusToActive()
    {
        // Arrange
        var device = Device.Create(DeviceId.Create("device-001"), "Test Device", "Sensor");
        device.Activate();
        device.StartMaintenance();

        // Act
        device.EndMaintenance();

        // Assert
        device.Status.Should().Be(DeviceStatus.Active);
        device.LastModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void EndMaintenance_WhenNotInMaintenance_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var device = Device.Create(DeviceId.Create("device-001"), "Test Device", "Sensor");

        // Act
        var act = () => device.EndMaintenance();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Device is not in maintenance mode*");
    }

    [Fact]
    public void Decommission_ShouldChangeStatusToDecommissioned()
    {
        // Arrange
        var device = Device.Create(DeviceId.Create("device-001"), "Test Device", "Sensor");
        device.Activate();

        // Act
        device.Decommission();

        // Assert
        device.Status.Should().Be(DeviceStatus.Decommissioned);
        device.LastModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void RecordActivity_ShouldUpdateLastSeenAt()
    {
        // Arrange
        var device = Device.Create(DeviceId.Create("device-001"), "Test Device", "Sensor");

        // Act
        device.RecordActivity();

        // Assert
        device.LastSeenAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateName()
    {
        // Arrange
        var device = Device.Create(DeviceId.Create("device-001"), "Old Name", "Sensor");

        // Act
        device.UpdateName("New Name");

        // Assert
        device.Name.Should().Be("New Name");
        device.LastModifiedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void UpdateName_WithEmptyName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        var device = Device.Create(DeviceId.Create("device-001"), "Test Device", "Sensor");

        // Act
        var act = () => device.UpdateName(invalidName!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Device name cannot be empty*");
    }

    [Fact]
    public void UpdateLocation_WithValidLocation_ShouldUpdateLocation()
    {
        // Arrange
        var device = Device.Create(DeviceId.Create("device-001"), "Test Device", "Sensor");

        // Act
        device.UpdateLocation("Building A, Floor 2");

        // Assert
        device.Location.Should().Be("Building A, Floor 2");
        device.LastModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateLocation_WithNull_ShouldClearLocation()
    {
        // Arrange
        var device = Device.Create(DeviceId.Create("device-001"), "Test Device", "Sensor");
        device.UpdateLocation("Building A");

        // Act
        device.UpdateLocation(null);

        // Assert
        device.Location.Should().BeNull();
    }

    [Fact]
    public void SetProperty_WithValidKeyValue_ShouldAddProperty()
    {
        // Arrange
        var device = Device.Create(DeviceId.Create("device-001"), "Test Device", "Sensor");

        // Act
        device.SetProperty("manufacturer", "Acme Corp");

        // Assert
        device.Properties.Should().ContainKey("manufacturer");
        device.Properties["manufacturer"].Should().Be("Acme Corp");
        device.LastModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void SetProperty_WithExistingKey_ShouldUpdateProperty()
    {
        // Arrange
        var device = Device.Create(DeviceId.Create("device-001"), "Test Device", "Sensor");
        device.SetProperty("firmware", "v1.0");

        // Act
        device.SetProperty("firmware", "v2.0");

        // Assert
        device.Properties["firmware"].Should().Be("v2.0");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void SetProperty_WithEmptyKey_ShouldThrowArgumentException(string invalidKey)
    {
        // Arrange
        var device = Device.Create(DeviceId.Create("device-001"), "Test Device", "Sensor");

        // Act
        var act = () => device.SetProperty(invalidKey!, "value");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Property key cannot be empty*");
    }

    [Fact]
    public void RemoveProperty_WithExistingKey_ShouldRemoveProperty()
    {
        // Arrange
        var device = Device.Create(DeviceId.Create("device-001"), "Test Device", "Sensor");
        device.SetProperty("temp", "value");

        // Act
        device.RemoveProperty("temp");

        // Assert
        device.Properties.Should().NotContainKey("temp");
        device.LastModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void CanSendTelemetry_WhenActive_ShouldReturnTrue()
    {
        // Arrange
        var device = Device.Create(DeviceId.Create("device-001"), "Test Device", "Sensor");
        device.Activate();

        // Act
        var canSend = device.CanSendTelemetry();

        // Assert
        canSend.Should().BeTrue();
    }

    [Fact]
    public void CanSendTelemetry_WhenInMaintenance_ShouldReturnTrue()
    {
        // Arrange
        var device = Device.Create(DeviceId.Create("device-001"), "Test Device", "Sensor");
        device.Activate();
        device.StartMaintenance();

        // Act
        var canSend = device.CanSendTelemetry();

        // Assert
        canSend.Should().BeTrue();
    }

    [Theory]
    [InlineData(DeviceStatus.Registered)]
    [InlineData(DeviceStatus.Inactive)]
    [InlineData(DeviceStatus.Disabled)]
    [InlineData(DeviceStatus.Decommissioned)]
    public void CanSendTelemetry_WhenNotActiveOrMaintenance_ShouldReturnFalse(DeviceStatus status)
    {
        // Arrange
        var device = Device.Create(DeviceId.Create("device-001"), "Test Device", "Sensor");

        switch (status)
        {
            case DeviceStatus.Inactive:
                device.Activate();
                device.Deactivate();
                break;
            case DeviceStatus.Disabled:
                device.Activate();
                device.Disable();
                break;
            case DeviceStatus.Decommissioned:
                device.Decommission();
                break;
        }

        // Act
        var canSend = device.CanSendTelemetry();

        // Assert
        canSend.Should().BeFalse();
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var device = Device.Create(DeviceId.Create("device-001"), "Test Device", "Sensor");
        device.DomainEvents.Should().HaveCount(1);

        // Act
        device.ClearDomainEvents();

        // Assert
        device.DomainEvents.Should().BeEmpty();
    }
}
