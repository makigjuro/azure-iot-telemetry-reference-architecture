using IoTTelemetry.Domain.Entities;
using IoTTelemetry.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using TelemetryProcessor.Application.Commands;
using TelemetryProcessor.Application.Handlers;
using TelemetryProcessor.Application.Ports;

namespace TelemetryProcessor.Application.Tests.Handlers;

public class ProcessTelemetryHandlerTests
{
    private readonly ITelemetryStorage _storage;
    private readonly ILogger<ProcessTelemetryHandler> _logger;
    private readonly ProcessTelemetryHandler _handler;

    public ProcessTelemetryHandlerTests()
    {
        _storage = Substitute.For<ITelemetryStorage>();
        _logger = Substitute.For<ILogger<ProcessTelemetryHandler>>();
        _handler = new ProcessTelemetryHandler(_storage, _logger);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldStoreToBronzeLayer()
    {
        // Arrange
        var reading = CreateValidReading();
        var command = new ProcessTelemetryCommand(
            reading,
            PartitionId: "0",
            SequenceNumber: 12345);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _storage.Received(1).StoreBronzeAsync(
            Arg.Is<TelemetryReading>(r => r.Id == reading.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnValidateTelemetryCommand()
    {
        // Arrange
        var reading = CreateValidReading();
        var command = new ProcessTelemetryCommand(
            reading,
            PartitionId: "0",
            SequenceNumber: 12345);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ValidateTelemetryCommand>();
        result.Reading.Should().Be(reading);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldLogInformation()
    {
        // Arrange
        var reading = CreateValidReading();
        var command = new ProcessTelemetryCommand(
            reading,
            PartitionId: "0",
            SequenceNumber: 12345);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains(reading.DeviceId.Value)),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_WhenStorageFails_ShouldPropagateException()
    {
        // Arrange
        var reading = CreateValidReading();
        var command = new ProcessTelemetryCommand(
            reading,
            PartitionId: "0",
            SequenceNumber: 12345);

        _storage.StoreBronzeAsync(Arg.Any<TelemetryReading>(), Arg.Any<CancellationToken>())
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
        var command = new ProcessTelemetryCommand(
            reading,
            PartitionId: "0",
            SequenceNumber: 12345);
        using var cts = new CancellationTokenSource();

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        await _storage.Received(1).StoreBronzeAsync(
            Arg.Any<TelemetryReading>(),
            Arg.Is<CancellationToken>(ct => ct == cts.Token));
    }

    [Fact]
    public async Task Handle_WithMultipleReadings_ShouldStoreEachSeparately()
    {
        // Arrange
        var reading1 = CreateValidReading();
        var reading2 = CreateValidReading();
        var command1 = new ProcessTelemetryCommand(reading1, "0", 1);
        var command2 = new ProcessTelemetryCommand(reading2, "1", 2);

        // Act
        await _handler.Handle(command1, CancellationToken.None);
        await _handler.Handle(command2, CancellationToken.None);

        // Assert
        await _storage.Received(1).StoreBronzeAsync(
            Arg.Is<TelemetryReading>(r => r.Id == reading1.Id),
            Arg.Any<CancellationToken>());
        await _storage.Received(1).StoreBronzeAsync(
            Arg.Is<TelemetryReading>(r => r.Id == reading2.Id),
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
