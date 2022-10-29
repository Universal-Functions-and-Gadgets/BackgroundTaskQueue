using QueuedBackgroundWorker;
using UFG.BackgroundTaskQueue.DependencyInjection;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddTaskQueue()
            .AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();