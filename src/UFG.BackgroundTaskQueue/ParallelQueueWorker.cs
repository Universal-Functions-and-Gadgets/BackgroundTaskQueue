using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace UFG.BackgroundTaskQueue;

internal sealed class ParallelQueueWorker(
    ITaskQueue taskQueue,
    IServiceProvider serviceProvider,
    ILogger<ParallelQueueWorker> logger,
    ParallelWorkerSettings settings) : IHostedService, IDisposable
{
    private static readonly TimeSpan s_cycleDelay = TimeSpan.FromMilliseconds(50);

#if NET8_0_OR_GREATER
    private ImmutableList<(Guid Id, IServiceScope Scope, Task Task)> _tasks = [];
#else
    private ImmutableList<(Guid Id, IServiceScope Scope, Task Task)> _tasks = ImmutableList<(Guid Id, IServiceScope Scope, Task Task)>.Empty;
#endif
    private bool _isRunning = true;
    private Func<bool> _hasItems = () => false;
    private Task _process = Task.CompletedTask;
    private Task _cleanUp = Task.CompletedTask;
    private readonly CancellationTokenSource _stopCts = new();
    private readonly CancellationTokenSource _readCts = new();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();

        logger.LogInformation($"Starting {nameof(ParallelQueueWorker)}");

        _hasItems = settings.StopBehavior == ParallelWorkerStopBehavior.Drain
            ? () => taskQueue.Count > 0
            : () => !_tasks.IsEmpty;

        _process = ProcessQueueAsync();
        _cleanUp = CleanUpAsync();
    }

    private async Task ProcessQueueAsync()
    {
        var nextLog = DateTime.MinValue;

        try
        {
            await foreach (var workItem in taskQueue.DequeueStreamAsync(_readCts.Token))
            {
                var scope = serviceProvider.CreateScope();

                _tasks = _tasks.Add((Guid.NewGuid(), scope, workItem(scope, _stopCts.Token)));

                logger.LogDebug("Task count {Count} / {ConcurrentLimit}", _tasks.Count, settings.ConcurrentLimit);

                while (_tasks.Count >= settings.ConcurrentLimit)
                {
                    if (DateTime.UtcNow > nextLog)
                    {
                        logger.LogDebug("Task count reached limit of {Limit}", settings.ConcurrentLimit);
                        nextLog = DateTime.UtcNow.AddSeconds(10);
                    }
                    await Task.Delay(s_cycleDelay, _readCts.Token);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }

        logger.LogInformation("Processing task finished");
    }

    private async Task CleanUpAsync()
    {
        while (_isRunning || _hasItems())
        {
            var completed = _tasks.Where(x => x.Task.IsCompleted).ToImmutableList();

            completed
                .ForEach(x =>
                {
                    if (x.Task.IsFaulted)
                    {
                        logger.LogError(new EventId(101), x.Task.Exception, "Queued task faulted");
                    }
                    x.Scope.Dispose();
                });

            var completedIds = completed.Select(x => x.Id).ToImmutableHashSet();
            _tasks = _tasks.Where(x => !completedIds.Contains(x.Id)).ToImmutableList();

            await Task.Delay(TimeSpan.FromMilliseconds(25), _stopCts.Token);
        }

        logger.LogInformation("CleanUp task finished");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation($"Stopping {nameof(ParallelQueueWorker)}");
        _isRunning = false;

        if (settings.StopBehavior == ParallelWorkerStopBehavior.Abandon)
        {
            logger.LogInformation("Abandoning all tasks");
            _stopCts.Cancel();
            _readCts.Cancel();
        }
        else
        {
            await GracefulStopAsync(cancellationToken);
        }

        logger.LogInformation($"Stopped {nameof(ParallelQueueWorker)}");
    }

    private async Task GracefulStopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Waiting for tasks to complete");

        if (settings.StopBehavior == ParallelWorkerStopBehavior.CompleteInFlight)
        {
            _readCts.Cancel();
        }

        _stopCts.CancelAfter(settings.StopTimeout);

        while (_hasItems())
        {
            await Task.Delay(s_cycleDelay, cancellationToken);
        }

        logger.LogInformation("Tasks completed");

        if (!_readCts.IsCancellationRequested)
        {
            _readCts.Cancel();
        }

        await Task.WhenAll(_process, _cleanUp);
    }

    public void Dispose()
    {
        _readCts.Dispose();
        _stopCts.Dispose();
    }
}
