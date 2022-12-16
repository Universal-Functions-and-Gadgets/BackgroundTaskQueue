using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace UFG.BackgroundTaskQueue.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="QueueWorker"/> as a hosted service and <see cref="ITaskQueue"/> to the service collection
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/></param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining calls</returns>
    public static IServiceCollection AddTaskQueue(this IServiceCollection services)
    {
        services.TryAddSingleton<ITaskQueue, ChannelTaskQueue>();
        
        return services.AddHostedService<QueueWorker>();
    }

    /// <summary>
    /// Adds <see cref="QueueWorker"/> as a hosted service and <see cref="ITaskQueue"/> to the service collection with
    /// a max queue capacity
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/></param>
    /// <param name="capacity">Max capacity of the task queue</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining calls</returns>
    public static IServiceCollection AddTaskQueue(this IServiceCollection services, int capacity) =>
        services.AddHostedService<QueueWorker>()
            .AddSingleton<ITaskQueue>(new ChannelTaskQueue(capacity));

    /// <summary>
    /// Adds <see cref="QueueWorker"/> as a hosted service and <see cref="ITaskQueue"/> to the service collection with
    /// specified channel options
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/></param>
    /// <param name="options">The <see cref="BoundedChannelOptions"/> to use</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining calls</returns>
    public static IServiceCollection AddTaskQueue(this IServiceCollection services, BoundedChannelOptions options) =>
        services.AddHostedService<QueueWorker>()
            .AddSingleton<ITaskQueue>(new ChannelTaskQueue(options));
}