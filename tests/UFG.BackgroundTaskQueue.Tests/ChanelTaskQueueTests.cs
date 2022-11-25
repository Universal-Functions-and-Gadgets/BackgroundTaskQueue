using System.Threading.Channels;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace UFG.BackgroundTaskQueue.Tests;

public class ChanelTaskQueueTests
{
    [Fact(DisplayName = "Creates default task queue")]
    public void Constructor1()
    {
        var queue = new ChannelTaskQueue();
        queue.Should().NotBeNull();
    }

    [Fact(DisplayName = "Creates task queue with capacity")]
    public void Constructor2()
    {
        var queue = new ChannelTaskQueue(1);
        queue.Should().NotBeNull();
    }
    
    [Fact(DisplayName = "Creates task queue with given options")]
    public void Constructor3()
    {
        var queue = new ChannelTaskQueue(new BoundedChannelOptions(1));
        queue.Should().NotBeNull();
    }

    [Fact(DisplayName = "Enqueue into and dequeue out of queue")]
    public async Task EnqueueDequeue1()
    {
        var wasFuncCalled = false;
        var queue = new ChannelTaskQueue();
        Func<IServiceScope, CancellationToken, Task> func = (IServiceScope _, CancellationToken _) =>
        {
            wasFuncCalled = true;
            return Task.CompletedTask;
        };

        await queue.EnqueueAsync(func);
        var enqueued = await queue.DequeueAsync(CancellationToken.None);
        
        await enqueued(null, CancellationToken.None);

        wasFuncCalled.Should().BeTrue();
    }
}