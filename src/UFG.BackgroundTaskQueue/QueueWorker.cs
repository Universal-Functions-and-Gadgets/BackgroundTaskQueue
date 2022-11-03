using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace UFG.BackgroundTaskQueue;

public class QueueWorker : BackgroundService
{
    private readonly ITaskQueue _taskQueue;
    private readonly IServiceProvider _serviceProvider;

    public QueueWorker(ITaskQueue taskQueue, IServiceProvider serviceProvider)
    {
        _taskQueue = taskQueue;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var workItem = await _taskQueue.DequeueAsync(stoppingToken);

            using var scope = _serviceProvider.CreateScope();

            await workItem(scope, stoppingToken);
        }
    }
}