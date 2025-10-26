using IoTTelemetry.Domain.Entities;
using IoTTelemetry.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using TelemetryProcessor.Application.Commands;
using TelemetryProcessor.Application.Handlers;
using TelemetryProcessor.Application.Ports;

namespace TelemetryProcessor.Application.Tests.Handlers;

public class StoreTelemetryHandlerTests
{
    private readonly ITelemetryStorage _storage;
    private readonly ILogger<StoreTelemetryHandler> _logger;
    private readonly StoreTelemetryHandler _handler;

    public StoreTelemetryHandlerTests()
    {
        _storage = Substitute.For<ITelemetryStorage>();
        _logger = Substitute.For<ILogger<StoreTelemetryHandler>>();
        _handler = new StoreTelemetryHandler(_storage, _logger);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldStoreToSilverLayer()
    {
        // Arrange
        var reading = CreateValidReading();
        var metadata = new Dictionary<string, string>
        {
            ["location"] = "Building A",
            ["floor"] = "3"
        };
        var command = new StoreTelemetryCommand(reading, metadata);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _storage.Received(1).StoreSilverAsync(
            Arg.Is<TelemetryReading>(r => r.Id == reading.Id),
            Arg.Is<Dictionary<string, string>>(m => m.Count == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithMetadata_ShouldPassMetadataToStorage()
    {
        // Arrange
        var reading = CreateValidReading();
        var metadata = new Dictionary<string, string>
        {
            ["location"] = "Building A",
            ["model"] = "TempSensor-v2"
        };
        var command = new StoreTelemetryCommand(reading, metadata);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _storage.Received(1).StoreSilverAsync(
            Arg.Any<TelemetryReading>(),
            Arg.Is<Dictionary<string, string>>(m =>
                m["location"] == "Building A" &&
                m["model"] == "TempSensor-v2"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNullMetadata_ShouldPassEmptyDictionary()
    {
        // Arrange
        var reading = CreateValidReading();
        var command = new StoreTelemetryCommand(reading, null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _storage.Received(1).StoreSilverAsync(
            Arg.Any<TelemetryReading>(),
            Arg.Is<Dictionary<string, string>>(m => m.Count == 0),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyMetadata_ShouldPassEmptyDictionary()
    {
        // Arrange
        var reading = CreateValidReading();
        var metadata = new Dictionary<string, string>();
        var command = new StoreTelemetryCommand(reading, metadata);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _storage.Received(1).StoreSilverAsync(
            Arg.Any<TelemetryReading>(),
            Arg.Is<Dictionary<string, string>>(m => m.Count == 0),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldLogInformation()
    {
        // Arrange
        var reading = CreateValidReading();
        var command = new StoreTelemetryCommand(reading, new Dictionary<string, string>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Successfully processed")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldLogDebug()
    {
        // Arrange
        var reading = CreateValidReading();
        var command = new StoreTelemetryCommand(reading, new Dictionary<string, string>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _logger.Received().Log(
            LogLevel.Debug,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Storing telemetry")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_WhenStorageFails_ShouldPropagateException()
    {
        // Arrange
        var reading = CreateValidReading();
        var command = new StoreTelemetryCommand(reading, new Dictionary<string, string>());
        _storage.StoreSilverAsync(
                Arg.Any<TelemetryReading>(),
                Arg.Any<Dictionary<string, string>>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Storage failure"));

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Storage failure");
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToStorage()
    {
        // Arrange
        var reading = CreateValidReading();
        var command = new StoreTelemetryCommand(reading, new Dictionary<string, string>());
        using var cts = new CancellationTokenSource();

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        await _storage.Received(1).StoreSilverAsync(
            Arg.Any<TelemetryReading>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Is<CancellationToken>(ct => ct == cts.Token));
    }

    [Fact]
    public async Task Handle_WithMultipleReadings_ShouldStoreEachSeparately()
    {
        // Arrange
        var reading1 = CreateValidReading();
        var reading2 = CreateValidReading();
        var command1 = new StoreTelemetryCommand(reading1, new Dictionary<string, string>());
        var command2 = new StoreTelemetryCommand(reading2, new Dictionary<string, string>());

        // Act
        await _handler.Handle(command1, CancellationToken.None);
        await _handler.Handle(command2, CancellationToken.None);

        // Assert
        await _storage.Received(1).StoreSilverAsync(
            Arg.Is<TelemetryReading>(r => r.Id == reading1.Id),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<CancellationToken>());
        await _storage.Received(1).StoreSilverAsync(
            Arg.Is<TelemetryReading>(r => r.Id == reading2.Id),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithComplexMetadata_ShouldPreserveAllFields()
    {
        // Arrange
        var reading = CreateValidReading();
        var metadata = new Dictionary<string, string>
        {
            ["location"] = "Building A",
            ["floor"] = "3",
            ["model"] = "TempSensor-v2",
            ["manufacturer"] = "Contoso",
            ["installDate"] = "2024-01-01",
            ["firmware"] = "1.2.3"
        };
        var command = new StoreTelemetryCommand(reading, metadata);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _storage.Received(1).StoreSilverAsync(
            Arg.Any<TelemetryReading>(),
            Arg.Is<Dictionary<string, string>>(m =>
                m.Count == 6 &&
                m["location"] == "Building A" &&
                m["floor"] == "3" &&
                m["model"] == "TempSensor-v2" &&
                m["manufacturer"] == "Contoso" &&
                m["installDate"] == "2024-01-01" &&
                m["firmware"] == "1.2.3"),
            Arg.Any<CancellationToken>());
    }

    private static TelemetryReading CreateValidReading()
    {
        var measurements = new Dictionary<string, TelemetryValue>
        {
            ["temperature"] = TelemetryValue.Create(25.5, "°C"),
            ["humidity"] = TelemetryValue.Create(60.0, "%")
        };

        return TelemetryReading.Create(
            DeviceId.Create($"device-{Guid.NewGuid()}"),
            Timestamp.Now(),
            measurements);
    }
}
