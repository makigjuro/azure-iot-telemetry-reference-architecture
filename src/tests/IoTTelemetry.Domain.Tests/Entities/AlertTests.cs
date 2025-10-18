using IoTTelemetry.Domain.Entities;
using IoTTelemetry.Domain.ValueObjects;

namespace IoTTelemetry.Domain.Tests.Entities;

public class AlertTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateAlert()
    {
        // Arrange
        var deviceId = DeviceId.Create("device-001");
        var timestamp = Timestamp.Now();
        const string message = "Temperature exceeds threshold";

        // Act
        var alert = Alert.Create(deviceId, AlertSeverity.Warning, message, timestamp);

        // Assert
        alert.DeviceId.Should().Be(deviceId);
        alert.Severity.Should().Be(AlertSeverity.Warning);
        alert.Message.Should().Be(message);
        alert.Timestamp.Should().Be(timestamp);
        alert.IsAcknowledged.Should().BeFalse();
        alert.AcknowledgedAt.Should().BeNull();
        alert.AcknowledgedBy.Should().BeNull();
        alert.CreatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Create_WithMetadata_ShouldIncludeMetadata()
    {
        // Arrange
        var metadata = new Dictionary<string, object>
        {
            ["threshold"] = 100.0,
            ["actual"] = 125.5
        };

        // Act
        var alert = Alert.Create(
            DeviceId.Create("device-001"),
            AlertSeverity.Error,
            "Test alert",
            Timestamp.Now(),
            metadata);

        // Assert
        alert.Metadata.Should().HaveCount(2);
        alert.Metadata["threshold"].Should().Be(100.0);
        alert.Metadata["actual"].Should().Be(125.5);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyMessage_ShouldThrowArgumentException(string invalidMessage)
    {
        // Arrange
        var deviceId = DeviceId.Create("device-001");

        // Act
        var act = () => Alert.Create(deviceId, AlertSeverity.Info, invalidMessage!, Timestamp.Now());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Alert message cannot be empty*");
    }

    [Fact]
    public void Acknowledge_WithValidParameters_ShouldAcknowledgeAlert()
    {
        // Arrange
        var alert = CreateUnacknowledgedAlert();
        const string acknowledgedBy = "admin@example.com";
        const string resolution = "Adjusted threshold";

        // Act
        alert.Acknowledge(acknowledgedBy, resolution);

        // Assert
        alert.IsAcknowledged.Should().BeTrue();
        alert.AcknowledgedBy.Should().Be(acknowledgedBy);
        alert.Resolution.Should().Be(resolution);
        alert.AcknowledgedAt.Should().NotBeNull();
    }

    [Fact]
    public void Acknowledge_WithoutResolution_ShouldStillAcknowledge()
    {
        // Arrange
        var alert = CreateUnacknowledgedAlert();

        // Act
        alert.Acknowledge("admin@example.com");

        // Assert
        alert.IsAcknowledged.Should().BeTrue();
        alert.Resolution.Should().BeNull();
    }

    [Fact]
    public void Acknowledge_WhenAlreadyAcknowledged_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var alert = CreateUnacknowledgedAlert();
        alert.Acknowledge("user1@example.com");

        // Act
        var act = () => alert.Acknowledge("user2@example.com");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Alert is already acknowledged*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Acknowledge_WithEmptyAcknowledgedBy_ShouldThrowArgumentException(string invalidAcknowledgedBy)
    {
        // Arrange
        var alert = CreateUnacknowledgedAlert();

        // Act
        var act = () => alert.Acknowledge(invalidAcknowledgedBy!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*AcknowledgedBy cannot be empty*");
    }

    [Fact]
    public void UpdateResolution_WhenAcknowledged_ShouldUpdateResolution()
    {
        // Arrange
        var alert = CreateUnacknowledgedAlert();
        alert.Acknowledge("admin@example.com", "Initial resolution");

        // Act
        alert.UpdateResolution("Updated resolution");

        // Assert
        alert.Resolution.Should().Be("Updated resolution");
    }

    [Fact]
    public void UpdateResolution_WhenNotAcknowledged_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var alert = CreateUnacknowledgedAlert();

        // Act
        var act = () => alert.UpdateResolution("Some resolution");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot update resolution for an unacknowledged alert*");
    }

    [Fact]
    public void AddMetadata_WithValidKeyValue_ShouldAddMetadata()
    {
        // Arrange
        var alert = CreateUnacknowledgedAlert();

        // Act
        alert.AddMetadata("source", "Stream Analytics");

        // Assert
        alert.Metadata.Should().ContainKey("source");
        alert.Metadata["source"].Should().Be("Stream Analytics");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void AddMetadata_WithEmptyKey_ShouldThrowArgumentException(string invalidKey)
    {
        // Arrange
        var alert = CreateUnacknowledgedAlert();

        // Act
        var act = () => alert.AddMetadata(invalidKey!, "value");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Metadata key cannot be empty*");
    }

    [Theory]
    [InlineData(AlertSeverity.Error)]
    [InlineData(AlertSeverity.Critical)]
    public void RequiresImmediateAction_WhenErrorOrCriticalAndNotAcknowledged_ShouldReturnTrue(AlertSeverity severity)
    {
        // Arrange
        var alert = Alert.Create(
            DeviceId.Create("device-001"),
            severity,
            "Test alert",
            Timestamp.Now());

        // Act
        var requiresAction = alert.RequiresImmediateAction();

        // Assert
        requiresAction.Should().BeTrue();
    }

    [Theory]
    [InlineData(AlertSeverity.Info)]
    [InlineData(AlertSeverity.Warning)]
    public void RequiresImmediateAction_WhenInfoOrWarning_ShouldReturnFalse(AlertSeverity severity)
    {
        // Arrange
        var alert = Alert.Create(
            DeviceId.Create("device-001"),
            severity,
            "Test alert",
            Timestamp.Now());

        // Act
        var requiresAction = alert.RequiresImmediateAction();

        // Assert
        requiresAction.Should().BeFalse();
    }

    [Fact]
    public void RequiresImmediateAction_WhenCriticalButAcknowledged_ShouldReturnFalse()
    {
        // Arrange
        var alert = Alert.Create(
            DeviceId.Create("device-001"),
            AlertSeverity.Critical,
            "Test alert",
            Timestamp.Now());
        alert.Acknowledge("admin@example.com");

        // Act
        var requiresAction = alert.RequiresImmediateAction();

        // Assert
        requiresAction.Should().BeFalse();
    }

    [Fact]
    public void GetAge_ShouldReturnTimeSinceCreation()
    {
        // Arrange
        var alert = CreateUnacknowledgedAlert();
        Thread.Sleep(100); // Small delay to ensure age > 0

        // Act
        var age = alert.GetAge();

        // Assert
        age.Should().BeGreaterThan(TimeSpan.Zero);
        age.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }

    private static Alert CreateUnacknowledgedAlert()
    {
        return Alert.Create(
            DeviceId.Create("device-001"),
            AlertSeverity.Warning,
            "Test alert",
            Timestamp.Now());
    }
}
