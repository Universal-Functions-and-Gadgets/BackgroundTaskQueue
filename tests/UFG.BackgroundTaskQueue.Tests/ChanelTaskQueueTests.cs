using System.Threading.Channels;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace UFG.BackgroundTaskQueue.Tests;

public class ChanelTaskQueueTests
{
    private static ILogger<ChannelTaskQueue> s_logger = new NullLoggerFactory().CreateLogger<ChannelTaskQueue>();

    [Fact(DisplayName = "Creates default task queue")]
    public void Constructor1()
    {
        var queue = new ChannelTaskQueue(s_logger);
        queue.Should().NotBeNull();
    }

    [Fact(DisplayName = "Creates task queue with capacity")]
    public void Constructor2()
    {
        var queue = new ChannelTaskQueue(1, s_logger);
        queue.Should().NotBeNull();
    }
    
    [Fact(DisplayName = "Creates task queue with given options")]
    public void Constructor3()
    {
        var queue = new ChannelTaskQueue(new BoundedChannelOptions(1), s_logger);
        queue.Should().NotBeNull();
    }

    [Fact(DisplayName = "Enqueue into and dequeue out of queue")]
    public async Task EnqueueDequeue1()
    {
        var wasFuncCalled = false;
        var queue = new ChannelTaskQueue(s_logger);
        Func<IServiceScope, CancellationToken, Task> func = (IServiceScope _, CancellationToken _) =>
        {
            wasFuncCalled = true;
            return Task.CompletedTask;
        };

        await queue.EnqueueAsync(func);
        var enqueued = await queue.DequeueAsync(CancellationToken.None);
        
        await enqueued(null!, CancellationToken.None);

        wasFuncCalled.Should().BeTrue();
    }
}