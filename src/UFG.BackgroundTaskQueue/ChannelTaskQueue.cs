using Microsoft.Extensions.DependencyInjection;

namespace UFG.BackgroundTaskQueue;

using System.Threading.Channels;

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
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleWriter = false,
            SingleReader = true
        };
        _queue = Channel.CreateBounded<Func<IServiceScope, CancellationToken, Task>>(options);
    }

    public ChannelTaskQueue(BoundedChannelOptions options)
    {
        _queue = Channel.CreateBounded<Func<IServiceScope, CancellationToken, Task>>(options);
    }

    public async ValueTask EnqueueAsync(Func<CancellationToken, Task> workItem)
    {
        if (workItem == null)
        {
            throw new ArgumentNullException(nameof(workItem));
        }

        await _queue.Writer.WriteAsync((_, ct) => workItem(ct));
    }

    public async ValueTask EnqueueAsync(Func<IServiceScope, CancellationToken, Task> workItem)
    {
        if (workItem == null)
        {
            throw new ArgumentNullException(nameof(workItem));
        }

        await _queue.Writer.WriteAsync(workItem);
    }

    public async ValueTask<Func<IServiceScope, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken) =>
        await _queue.Reader.ReadAsync(cancellationToken);
}