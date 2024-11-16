using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace UFG.BackgroundTaskQueue;

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
    private readonly Action<ILogger, Exception?> _logItemDropped;

    public ChannelTaskQueue(ILogger<ChannelTaskQueue> logger) : this(DefaultCapacity, logger)
    {
    }

    public ChannelTaskQueue(int capacity, ILogger<ChannelTaskQueue> logger)
        : this(
            new BoundedChannelOptions(capacity)
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
        _logItemDropped = LoggerMessage.Define(LogLevel.Warning, new EventId(100), "Task queue item dropped");
    }

    private void LogDropped(Func<IServiceScope, CancellationToken, Task> func)
    {
        _logItemDropped(_logger, null);
    }

    /// <inheritdoc />
    public ValueTask EnqueueAsync(Func<IServiceScope, CancellationToken, Task> workItem) =>
        _queue.Writer.WriteAsync(workItem);

    /// <inheritdoc />
    public ValueTask<Func<IServiceScope, CancellationToken, Task>> DequeueAsync(
        CancellationToken cancellationToken) =>
        _queue.Reader.ReadAsync(cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<Func<IServiceScope, CancellationToken, Task>> DequeueStreamAsync(
        CancellationToken cancellationToken) =>
        _queue.Reader.ReadAllAsync(cancellationToken);

    /// <inheritdoc />
    public int Count => _queue.Reader.Count;
}
