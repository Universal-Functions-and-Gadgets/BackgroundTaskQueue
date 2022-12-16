using UFG.BackgroundTaskQueue.DependencyInjection;
using UFG.Samples.ServiceWorker;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddTaskQueue()
            .AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();