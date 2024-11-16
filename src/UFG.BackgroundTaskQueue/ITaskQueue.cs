using Microsoft.Extensions.DependencyInjection;

namespace UFG.BackgroundTaskQueue;

#pragma warning disable CA1711
public interface ITaskQueue
#pragma warning restore CA1711
{
    /// <summary>
    /// Enqueues a work item encapsulated within a function
    /// </summary>
    /// <param name="workItem">The encapsulated work</param>
    /// <returns><see cref="ValueTask"/> for awaiting</returns>
    ValueTask EnqueueAsync(Func<IServiceScope, CancellationToken, Task> workItem);

    /// <summary>
    /// Dequeues a work item for execution
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> to stop dequeue operation</param>
    /// <returns>The next work item available</returns>
    ValueTask<Func<IServiceScope, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Dequeues work item as a stream
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> to stop dequeue operation</param>
    /// <returns>Stream of work items</returns>
    IAsyncEnumerable<Func<IServiceScope, CancellationToken, Task>> DequeueStreamAsync(
        CancellationToken cancellationToken);

    /// <summary>
    /// The number of items in the queue
    /// </summary>
    int Count { get; }
}
