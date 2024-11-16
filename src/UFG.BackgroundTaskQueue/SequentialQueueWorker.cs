using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace UFG.BackgroundTaskQueue;

internal sealed class SequentialQueueWorker(ITaskQueue taskQueue, IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var workItem = await taskQueue.DequeueAsync(stoppingToken);

            using var scope = serviceProvider.CreateScope();

            await workItem(scope, stoppingToken);
        }
    }
}
