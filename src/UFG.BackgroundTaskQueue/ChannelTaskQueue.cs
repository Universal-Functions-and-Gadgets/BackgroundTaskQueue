using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace UFG.BackgroundTaskQueue;

using System.Threading.Channels;

/// <summary>
/// Bounded <see cref="Channel{T}"/> based implementation of <see cref="ITaskQueue"/>
/// </summary>
#pragma warning disable CA1711
public class ChannelTaskQueue : ITaskQueue
#pragma warning restore CA1711
{
    private readonly ILogger<ChannelTaskQueue> _logger;
    private const int DefaultCapacity = 100;

    private readonly Channel<Func<IServiceScope, CancellationToken, Task>> _queue;

    public ChannelTaskQueue(ILogger<ChannelTaskQueue> logger) : this(DefaultCapacity, logger)
    {
    }

    public ChannelTaskQueue(int capacity, ILogger<ChannelTaskQueue> logger)
        : this(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleWriter = false,
                SingleReader = true
            },
            logger)
    {
    }

    public ChannelTaskQueue(BoundedChannelOptions options, ILogger<ChannelTaskQueue> logger)
    {
        _queue = Channel.CreateBounded<Func<IServiceScope, CancellationToken, Task>>(options, LogDropped);
        _logger = logger;
    }

    private void LogDropped(Func<IServiceScope, CancellationToken, Task> func)
    {
#pragma warning disable CA1848
        _logger.LogWarning("Task queue item dropped");
#pragma warning restore CA1848
    }

    /// <inheritdoc />
    public async ValueTask EnqueueAsync(Func<IServiceScope, CancellationToken, Task> workItem) =>
        await _queue.Writer.WriteAsync(workItem);

    /// <inheritdoc />
    public async ValueTask<Func<IServiceScope, CancellationToken, Task>> DequeueAsync(
        CancellationToken cancellationToken) =>
        await _queue.Reader.ReadAsync(cancellationToken);
}