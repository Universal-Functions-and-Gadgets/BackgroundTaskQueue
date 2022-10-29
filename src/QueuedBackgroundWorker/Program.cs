using QueuedBackgroundWorker;
using UFG.BackgroundTaskQueue.Extensions;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddTaskQueue()
            .AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();