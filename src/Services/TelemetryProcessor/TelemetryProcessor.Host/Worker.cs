namespace TelemetryProcessor.Host;

public partial class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            LogWorkerRunning(DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Worker running at: {Time}")]
    partial void LogWorkerRunning(DateTimeOffset time);
}
