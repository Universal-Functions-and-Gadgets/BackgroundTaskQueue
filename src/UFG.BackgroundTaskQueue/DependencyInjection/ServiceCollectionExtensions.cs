using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace UFG.BackgroundTaskQueue.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="SequentialQueueWorker"/> as a hosted service and <see cref="ITaskQueue"/> to the service collection
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/></param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining calls</returns>
    public static IServiceCollection AddTaskQueue(this IServiceCollection services)
    {
        services.TryAddSingleton<ITaskQueue, ChannelTaskQueue>();

        return services.AddHostedService<SequentialQueueWorker>();
    }

    /// <summary>
    /// Adds <see cref="SequentialQueueWorker"/> as a hosted service and <see cref="ITaskQueue"/> to the service collection with
    /// a max queue capacity
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/></param>
    /// <param name="capacity">Max capacity of the task queue</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining calls</returns>
    public static IServiceCollection AddTaskQueue(this IServiceCollection services, int capacity) =>
        services.AddHostedService<SequentialQueueWorker>()
            .AddSingleton<ITaskQueue>(sp =>
                new ChannelTaskQueue(capacity, sp.GetRequiredService<ILogger<ChannelTaskQueue>>()));

    /// <summary>
    /// Adds <see cref="SequentialQueueWorker"/> as a hosted service and <see cref="ITaskQueue"/> to the service collection with
    /// specified channel options
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/></param>
    /// <param name="options">The <see cref="BoundedChannelOptions"/> to use</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining calls</returns>
    public static IServiceCollection AddTaskQueue(this IServiceCollection services, BoundedChannelOptions options) =>
        services.AddHostedService<SequentialQueueWorker>()
            .AddSingleton<ITaskQueue>(sp =>
                new ChannelTaskQueue(options, sp.GetRequiredService<ILogger<ChannelTaskQueue>>()));

    /// <summary>
    /// Adds <see cref="ParallelQueueWorker"/> as a hosted service and <see cref="ITaskQueue"/> to the service collection
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/></param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining calls</returns>
    public static IServiceCollection AddParallelTaskQueue(this IServiceCollection services)
    {
        services.TryAddSingleton<ITaskQueue, ChannelTaskQueue>();
        services.TryAddSingleton(new ParallelWorkerSettings());

        return services.AddHostedService<ParallelQueueWorker>();
    }

    /// <summary>
    /// Adds <see cref="ParallelQueueWorker"/> as a hosted service and <see cref="ITaskQueue"/> to the service collection with
    /// a max queue capacity
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/></param>
    /// <param name="capacity">Max capacity of the task queue</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining calls</returns>
    public static IServiceCollection AddParallelTaskQueue(this IServiceCollection services, int capacity)
    {
        services.TryAddSingleton(new ParallelWorkerSettings());

        return services.AddHostedService<ParallelQueueWorker>()
            .AddSingleton<ITaskQueue>(sp =>
                new ChannelTaskQueue(capacity, sp.GetRequiredService<ILogger<ChannelTaskQueue>>()));
    }

    /// <summary>
    /// Adds <see cref="ParallelQueueWorker"/> as a hosted service and <see cref="ITaskQueue"/> to the service collection with
    /// a max queue capacity
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/></param>
    /// <param name="settings">Settings for the worker</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining calls</returns>
    public static IServiceCollection AddParallelTaskQueue(this IServiceCollection services, ParallelWorkerSettings settings)
    {
        services.TryAddSingleton<ITaskQueue, ChannelTaskQueue>();
        services.TryAddSingleton(settings);

        return services.AddHostedService<ParallelQueueWorker>();
    }

    /// <summary>
    /// Adds <see cref="ParallelQueueWorker"/> as a hosted service and <see cref="ITaskQueue"/> to the service collection with
    /// a max queue capacity
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/></param>
    /// <param name="capacity">Max capacity of the task queue</param>
    /// <param name="settings">Settings for the worker</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining calls</returns>
    public static IServiceCollection AddParallelTaskQueue(this IServiceCollection services, int capacity, ParallelWorkerSettings settings)
    {
        services.TryAddSingleton(settings);

        return services.AddHostedService<ParallelQueueWorker>()
            .AddSingleton<ITaskQueue>(sp =>
                new ChannelTaskQueue(capacity, sp.GetRequiredService<ILogger<ChannelTaskQueue>>()));
    }
}
