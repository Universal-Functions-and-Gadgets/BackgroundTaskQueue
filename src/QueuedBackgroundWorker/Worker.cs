using UFG.BackgroundTaskQueue;
using UFG.BackgroundTaskQueue.Extensions;

namespace QueuedBackgroundWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ITaskQueue _taskQueue;

    public Worker(ILogger<Worker> logger, ITaskQueue taskQueue)
    {
        _logger = logger;
        _taskQueue = taskQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;

            await _taskQueue.EnqueueAsync(() =>
            {
                var then = now;
                var newNow = DateTime.UtcNow;

#pragma warning disable CA1848
                _logger.LogInformation("Duration to complete {Duration}", newNow.Subtract(then));
#pragma warning restore CA1848
                return Task.CompletedTask;
            });
            await Task.Delay(1000, stoppingToken);
        }
    }
}