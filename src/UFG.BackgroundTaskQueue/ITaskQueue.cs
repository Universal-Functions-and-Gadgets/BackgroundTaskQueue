using Microsoft.Extensions.DependencyInjection;

namespace UFG.BackgroundTaskQueue;

#pragma warning disable CA1711
public interface ITaskQueue
#pragma warning restore CA1711
{
    ValueTask EnqueueAsync(Func<IServiceScope, CancellationToken, Task> workItem);
    ValueTask<Func<IServiceScope, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
}