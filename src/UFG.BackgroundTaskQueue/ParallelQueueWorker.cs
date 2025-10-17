using System.Collections.Concurrent;
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

    private (IServiceScope? Scope, Task? Task)?[] _tasks = [];
    private ConcurrentQueue<int> _availableIndices = new();
    private readonly object _tasksLock = new();
    private int _activeTaskCount;
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

        // Initialize the fixed-size array and available indices queue
        _tasks = new (IServiceScope? Scope, Task? Task)?[settings.ConcurrentLimit];
        _availableIndices = new ConcurrentQueue<int>(Enumerable.Range(0, settings.ConcurrentLimit));
        _activeTaskCount = 0;

        _hasItems = settings.StopBehavior == ParallelWorkerStopBehavior.Drain
            ? () => taskQueue.Count > 0
            : () =>
            {
                lock (_tasksLock)
                {
                    return _activeTaskCount > 0;
                }
            };

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
                // Wait for an available index
                int index;
                while (!_availableIndices.TryDequeue(out index))
                {
                    if (DateTime.UtcNow > nextLog)
                    {
                        logger.LogDebug("Task count reached limit of {Limit}", settings.ConcurrentLimit);
                        nextLog = DateTime.UtcNow.AddSeconds(10);
                    }
                    await Task.Delay(s_cycleDelay, _readCts.Token);
                }

                var scope = serviceProvider.CreateScope();
                var task = workItem(scope, _stopCts.Token);

                lock (_tasksLock)
                {
                    _tasks[index] = (scope, task);
                    _activeTaskCount++;
                }

                logger.LogDebug("Task count {Count} / {ConcurrentLimit}", _activeTaskCount, settings.ConcurrentLimit);
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
            for (var i = 0; i < _tasks.Length; i++)
            {
                (IServiceScope? Scope, Task? Task)? taskEntry;
                lock (_tasksLock)
                {
                    taskEntry = _tasks[i];
                }

                if (taskEntry.HasValue && taskEntry.Value.Task != null && taskEntry.Value.Task.IsCompleted)
                {
                    var task = taskEntry.Value.Task;
                    var scope = taskEntry.Value.Scope;

                    if (task.IsFaulted)
                    {
                        logger.LogError(new EventId(101), task.Exception, "Queued task faulted");
                    }

                    scope?.Dispose();

                    lock (_tasksLock)
                    {
                        _tasks[i] = null;
                        _activeTaskCount--;
                    }

                    _availableIndices.Enqueue(i);
                }
            }

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
