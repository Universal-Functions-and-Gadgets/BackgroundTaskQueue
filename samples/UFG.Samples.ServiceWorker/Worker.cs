using System.Diagnostics;
using UFG.BackgroundTaskQueue;
using UFG.BackgroundTaskQueue.Extensions;

namespace UFG.Samples.ServiceWorker;

#pragma warning disable CA1848
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
        var count = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;

            await _taskQueue.EnqueueAsync(async () =>
            {
                var idx = count;
                var sw = Stopwatch.StartNew();
                await Task.Delay(TimeSpan.FromSeconds(15));

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("[{Index}] Duration to complete {Duration}", idx, sw.Elapsed);
                }
            });
            await Task.Delay(100, stoppingToken);
            
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Added {Count} items", ++count);
            }
        }
    }
}