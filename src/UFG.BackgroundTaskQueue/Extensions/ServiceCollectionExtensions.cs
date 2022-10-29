using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;

namespace UFG.BackgroundTaskQueue.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTaskQueue(this IServiceCollection services) =>
        services.AddHostedService<QueueWorker>()
            .AddSingleton<ITaskQueue, ChannelTaskQueue>();

    public static IServiceCollection AddTaskQueue(this IServiceCollection services, int capacity) =>
        services.AddHostedService<QueueWorker>()
            .AddSingleton<ITaskQueue>(new ChannelTaskQueue(capacity));

    public static IServiceCollection AddTaskQueue(this IServiceCollection services, BoundedChannelOptions options) =>
        services.AddHostedService<QueueWorker>()
            .AddSingleton<ITaskQueue>(new ChannelTaskQueue(options));
}