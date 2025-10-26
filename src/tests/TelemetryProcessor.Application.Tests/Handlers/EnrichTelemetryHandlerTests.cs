using IoTTelemetry.Domain.Entities;
using IoTTelemetry.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using TelemetryProcessor.Application.Commands;
using TelemetryProcessor.Application.Handlers;
using TelemetryProcessor.Application.Ports;

namespace TelemetryProcessor.Application.Tests.Handlers;

public class EnrichTelemetryHandlerTests
{
    private readonly IDeviceMetadataRepository _metadataRepository;
    private readonly ILogger<EnrichTelemetryHandler> _logger;
    private readonly EnrichTelemetryHandler _handler;

    public EnrichTelemetryHandlerTests()
    {
        _metadataRepository = Substitute.For<IDeviceMetadataRepository>();
        _logger = Substitute.For<ILogger<EnrichTelemetryHandler>>();
        _handler = new EnrichTelemetryHandler(_metadataRepository, _logger);
    }

    [Fact]
    public async Task Handle_WithMetadataAvailable_ShouldReturnEnrichedEventAndStoreCommand()
    {
        // Arrange
        var reading = CreateValidReading();
        var command = new EnrichTelemetryCommand(reading);
        var metadata = new Dictionary<string, string>
        {
            ["location"] = "Building A",
            ["floor"] = "3",
            ["model"] = "TempSensor-v2"
        };
        _metadataRepository.GetMetadataAsync(Arg.Any<DeviceId>(), Arg.Any<CancellationToken>())
            .Returns(metadata);

        // Act
        var (enrichedEvent, storeCommand) = await _handler.Handle(command, CancellationToken.None);

        // Assert
        enrichedEvent.Should().NotBeNull();
        enrichedEvent.DeviceId.Should().Be(reading.DeviceId);
        enrichedEvent.TelemetryId.Should().Be(reading.Id);
        enrichedEvent.EnrichedMetadata.Should().BeEquivalentTo(metadata);

        storeCommand.Should().NotBeNull();
        storeCommand.Reading.Should().Be(reading);
        storeCommand.EnrichedMetadata.Should().BeEquivalentTo(metadata);
    }

    [Fact]
    public async Task Handle_WithNoMetadataAvailable_ShouldReturnEmptyMetadata()
    {
        // Arrange
        var reading = CreateValidReading();
        var command = new EnrichTelemetryCommand(reading);
        _metadataRepository.GetMetadataAsync(Arg.Any<DeviceId>(), Arg.Any<CancellationToken>())
            .Returns((Dictionary<string, string>?)null);

        // Act
        var (enrichedEvent, storeCommand) = await _handler.Handle(command, CancellationToken.None);

        // Assert
        enrichedEvent.Should().NotBeNull();
        enrichedEvent.EnrichedMetadata.Should().NotBeNull();
        enrichedEvent.EnrichedMetadata.Should().BeEmpty();

        storeCommand.Should().NotBeNull();
        storeCommand.EnrichedMetadata.Should().NotBeNull();
        storeCommand.EnrichedMetadata.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithNoMetadataAvailable_ShouldLogWarning()
    {
        // Arrange
        var reading = CreateValidReading();
        var command = new EnrichTelemetryCommand(reading);
        _metadataRepository.GetMetadataAsync(Arg.Any<DeviceId>(), Arg.Any<CancellationToken>())
            .Returns((Dictionary<string, string>?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _logger.Received().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("No metadata found")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_WithMetadataAvailable_ShouldLogDebug()
    {
        // Arrange
        var reading = CreateValidReading();
        var command = new EnrichTelemetryCommand(reading);
        var metadata = new Dictionary<string, string>
        {
            ["location"] = "Building A"
        };
        _metadataRepository.GetMetadataAsync(Arg.Any<DeviceId>(), Arg.Any<CancellationToken>())
            .Returns(metadata);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _logger.Received().Log(
            LogLevel.Debug,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Enriched telemetry")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_ShouldQueryRepositoryWithCorrectDeviceId()
    {
        // Arrange
        var deviceId = DeviceId.Create("test-device-123");
        var reading = CreateValidReading(deviceId);
        var command = new EnrichTelemetryCommand(reading);
        _metadataRepository.GetMetadataAsync(Arg.Any<DeviceId>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _metadataRepository.Received(1).GetMetadataAsync(
            Arg.Is<DeviceId>(d => d.Value == "test-device-123"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var reading = CreateValidReading();
        var command = new EnrichTelemetryCommand(reading);
        _metadataRepository.GetMetadataAsync(Arg.Any<DeviceId>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Repository error"));

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Repository error");
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        var reading = CreateValidReading();
        var command = new EnrichTelemetryCommand(reading);
        using var cts = new CancellationTokenSource();
        _metadataRepository.GetMetadataAsync(Arg.Any<DeviceId>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string>());

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        await _metadataRepository.Received(1).GetMetadataAsync(
            Arg.Any<DeviceId>(),
            Arg.Is<CancellationToken>(ct => ct == cts.Token));
    }

    [Fact]
    public async Task Handle_WithMultipleMetadataFields_ShouldIncludeAllInEvent()
    {
        // Arrange
        var reading = CreateValidReading();
        var command = new EnrichTelemetryCommand(reading);
        var metadata = new Dictionary<string, string>
        {
            ["location"] = "Building A",
            ["floor"] = "3",
            ["model"] = "TempSensor-v2",
            ["manufacturer"] = "Contoso",
            ["installDate"] = "2024-01-01"
        };
        _metadataRepository.GetMetadataAsync(Arg.Any<DeviceId>(), Arg.Any<CancellationToken>())
            .Returns(metadata);

        // Act
        var (enrichedEvent, _) = await _handler.Handle(command, CancellationToken.None);

        // Assert
        enrichedEvent.EnrichedMetadata.Should().HaveCount(5);
        enrichedEvent.EnrichedMetadata["location"].Should().Be("Building A");
        enrichedEvent.EnrichedMetadata["floor"].Should().Be("3");
        enrichedEvent.EnrichedMetadata["model"].Should().Be("TempSensor-v2");
        enrichedEvent.EnrichedMetadata["manufacturer"].Should().Be("Contoso");
        enrichedEvent.EnrichedMetadata["installDate"].Should().Be("2024-01-01");
    }

    [Fact]
    public async Task Handle_WithDifferentDevices_ShouldReturnCorrectMetadata()
    {
        // Arrange
        var device1 = DeviceId.Create("device-001");
        var device2 = DeviceId.Create("device-002");
        var reading1 = CreateValidReading(device1);
        var reading2 = CreateValidReading(device2);
        var command1 = new EnrichTelemetryCommand(reading1);
        var command2 = new EnrichTelemetryCommand(reading2);

        _metadataRepository.GetMetadataAsync(device1, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string> { ["location"] = "Building A" });
        _metadataRepository.GetMetadataAsync(device2, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string> { ["location"] = "Building B" });

        // Act
        var (event1, _) = await _handler.Handle(command1, CancellationToken.None);
        var (event2, _) = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        event1.EnrichedMetadata["location"].Should().Be("Building A");
        event2.EnrichedMetadata["location"].Should().Be("Building B");
    }

    private static TelemetryReading CreateValidReading(DeviceId? deviceId = null)
    {
        var measurements = new Dictionary<string, TelemetryValue>
        {
            ["temperature"] = TelemetryValue.Create(25.5, "°C"),
            ["humidity"] = TelemetryValue.Create(60.0, "%")
        };

        return TelemetryReading.Create(
            deviceId ?? DeviceId.Create($"device-{Guid.NewGuid()}"),
            Timestamp.Now(),
            measurements);
    }
}
