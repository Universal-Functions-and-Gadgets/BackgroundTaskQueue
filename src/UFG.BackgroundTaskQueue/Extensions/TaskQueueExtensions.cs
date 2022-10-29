namespace UFG.BackgroundTaskQueue.Extensions;

public static class TaskQueueExtensions
{
    /// <summary>
    /// Enqueues a work item encapsulated within a function
    /// </summary>
    /// <param name="queue">The queue to add to</param>
    /// <param name="workItem">The encapsulated work</param>
    /// <returns><see cref="ValueTask"/> for awaiting</returns>
    public static async ValueTask EnqueueAsync(this ITaskQueue queue, Func<Task> workItem) =>
        await queue.EnqueueAsync((_, _) => workItem());

    /// <summary>
    /// Enqueues a work item encapsulated within a function
    /// </summary>
    /// <param name="queue">The queue to add to</param>
    /// <param name="workItem">The encapsulated work</param>
    /// <returns><see cref="ValueTask"/> for awaiting</returns>
    public static async ValueTask EnqueueAsync(this ITaskQueue queue, Func<CancellationToken, Task> workItem) =>
        await queue.EnqueueAsync((_, ct) => workItem(ct));
}