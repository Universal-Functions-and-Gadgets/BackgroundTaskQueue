using UFG.BackgroundTaskQueue.DependencyInjection;
using UFG.Samples.ServiceWorker;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddParallelTaskQueue()
            .AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();