using IoTTelemetry.Domain.Entities;
using IoTTelemetry.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using TelemetryProcessor.Application.Commands;
using TelemetryProcessor.Application.Handlers;
using TelemetryProcessor.Application.Ports;

namespace TelemetryProcessor.Application.Tests.Handlers;

public class ValidateTelemetryHandlerTests
{
    private readonly ITelemetryValidator _validator;
    private readonly ILogger<ValidateTelemetryHandler> _logger;
    private readonly ValidateTelemetryHandler _handler;

    public ValidateTelemetryHandlerTests()
    {
        _validator = Substitute.For<ITelemetryValidator>();
        _logger = Substitute.For<ILogger<ValidateTelemetryHandler>>();
        _handler = new ValidateTelemetryHandler(_validator, _logger);
    }

    [Fact]
    public async Task Handle_WithValidTelemetry_ShouldReturnSuccessEventAndEnrichCommand()
    {
        // Arrange
        var reading = CreateValidReading();
        var command = new ValidateTelemetryCommand(reading);
        _validator.ValidateAsync(Arg.Any<TelemetryReading>())
            .Returns(ValidationResult.Success());

        // Act
        var (validatedEvent, enrichCommand) = await _handler.Handle(command, CancellationToken.None);

        // Assert
        validatedEvent.Should().NotBeNull();
        validatedEvent.DeviceId.Should().Be(reading.DeviceId);
        validatedEvent.TelemetryId.Should().Be(reading.Id);
        validatedEvent.IsValid.Should().BeTrue();
        validatedEvent.ValidationError.Should().BeNull();

        enrichCommand.Should().NotBeNull();
        enrichCommand!.Reading.Should().Be(reading);
    }

    [Fact]
    public async Task Handle_WithInvalidTelemetry_ShouldReturnFailureEventAndNullCommand()
    {
        // Arrange
        var reading = CreateValidReading();
        var command = new ValidateTelemetryCommand(reading);
        var errorMessage = "Telemetry contains bad quality measurements";
        _validator.ValidateAsync(Arg.Any<TelemetryReading>())
            .Returns(ValidationResult.Failure(errorMessage));

        // Act
        var (validatedEvent, enrichCommand) = await _handler.Handle(command, CancellationToken.None);

        // Assert
        validatedEvent.Should().NotBeNull();
        validatedEvent.DeviceId.Should().Be(reading.DeviceId);
        validatedEvent.TelemetryId.Should().Be(reading.Id);
        validatedEvent.IsValid.Should().BeFalse();
        validatedEvent.ValidationError.Should().Be(errorMessage);

        enrichCommand.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithInvalidTelemetry_ShouldMarkReadingAsInvalid()
    {
        // Arrange
        var reading = CreateValidReading();
        var command = new ValidateTelemetryCommand(reading);
        var errorMessage = "Telemetry is too old";
        _validator.ValidateAsync(Arg.Any<TelemetryReading>())
            .Returns(ValidationResult.Failure(errorMessage));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        reading.IsValid.Should().BeFalse();
        reading.ValidationError.Should().Be(errorMessage);
    }

    [Fact]
    public async Task Handle_WithValidTelemetry_ShouldNotMarkReadingAsInvalid()
    {
        // Arrange
        var reading = CreateValidReading();
        var command = new ValidateTelemetryCommand(reading);
        _validator.ValidateAsync(Arg.Any<TelemetryReading>())
            .Returns(ValidationResult.Success());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        reading.IsValid.Should().BeTrue();
        reading.ValidationError.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithValidTelemetry_ShouldLogDebug()
    {
        // Arrange
        var reading = CreateValidReading();
        var command = new ValidateTelemetryCommand(reading);
        _validator.ValidateAsync(Arg.Any<TelemetryReading>())
            .Returns(ValidationResult.Success());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _logger.Received().Log(
            LogLevel.Debug,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("validated successfully")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_WithInvalidTelemetry_ShouldLogWarning()
    {
        // Arrange
        var reading = CreateValidReading();
        var command = new ValidateTelemetryCommand(reading);
        var errorMessage = "Validation failed";
        _validator.ValidateAsync(Arg.Any<TelemetryReading>())
            .Returns(ValidationResult.Failure(errorMessage));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _logger.Received().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("validation failed")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_WhenValidatorThrows_ShouldPropagateException()
    {
        // Arrange
        var reading = CreateValidReading();
        var command = new ValidateTelemetryCommand(reading);
        _validator.ValidateAsync(Arg.Any<TelemetryReading>())
            .ThrowsAsync(new InvalidOperationException("Validator error"));

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Validator error");
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToValidator()
    {
        // Arrange
        var reading = CreateValidReading();
        var command = new ValidateTelemetryCommand(reading);
        using var cts = new CancellationTokenSource();
        _validator.ValidateAsync(Arg.Any<TelemetryReading>())
            .Returns(ValidationResult.Success());

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        await _validator.Received(1).ValidateAsync(Arg.Any<TelemetryReading>());
    }

    [Fact]
    public async Task Handle_WithDifferentErrors_ShouldReturnAppropriateEvents()
    {
        // Arrange
        var reading1 = CreateValidReading();
        var reading2 = CreateValidReading();
        var command1 = new ValidateTelemetryCommand(reading1);
        var command2 = new ValidateTelemetryCommand(reading2);

        _validator.ValidateAsync(reading1)
            .Returns(ValidationResult.Failure("Error 1"));
        _validator.ValidateAsync(reading2)
            .Returns(ValidationResult.Failure("Error 2"));

        // Act
        var (event1, _) = await _handler.Handle(command1, CancellationToken.None);
        var (event2, _) = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        event1.ValidationError.Should().Be("Error 1");
        event2.ValidationError.Should().Be("Error 2");
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
