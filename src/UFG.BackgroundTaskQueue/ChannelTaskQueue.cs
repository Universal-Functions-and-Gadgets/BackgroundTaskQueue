using Microsoft.Extensions.DependencyInjection;

namespace UFG.BackgroundTaskQueue;

using System.Threading.Channels;

/// <summary>
/// Bounded <see cref="Channel{T}"/> based implementation of <see cref="ITaskQueue"/>
/// </summary>
#pragma warning disable CA1711
public class ChannelTaskQueue : ITaskQueue
#pragma warning restore CA1711
{
    private const int DefaultCapacity = 100;

    private readonly Channel<Func<IServiceScope, CancellationToken, Task>> _queue;

    public ChannelTaskQueue() : this(DefaultCapacity)
    {
    }

    public ChannelTaskQueue(int capacity) 
        : this(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleWriter = false,
            SingleReader = true
        })
    {
    }

    public ChannelTaskQueue(BoundedChannelOptions options)
    {
        _queue = Channel.CreateBounded<Func<IServiceScope, CancellationToken, Task>>(options);
    }

    /// <inheritdoc />
    public async ValueTask EnqueueAsync(Func<IServiceScope, CancellationToken, Task> workItem) =>
        await _queue.Writer.WriteAsync(workItem);

    /// <inheritdoc />
    public async ValueTask<Func<IServiceScope, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken) =>
        await _queue.Reader.ReadAsync(cancellationToken);
}