namespace UFG.BackgroundTaskQueue.Extensions;

public static class TaskQueueExtensions
{
    public static async ValueTask EnqueueAsync(this ITaskQueue queue, Func<Task> workItem)
    {
        if (workItem == null)
        {
            throw new ArgumentNullException(nameof(workItem));
        }

        await queue.EnqueueAsync((_, _) => workItem());
    }

    public static async ValueTask EnqueueAsync(this ITaskQueue queue, Func<CancellationToken, Task> workItem)
    {
        if (workItem == null)
        {
            throw new ArgumentNullException(nameof(workItem));
        }

        await queue.EnqueueAsync((_, ct) => workItem(ct));
    }
}