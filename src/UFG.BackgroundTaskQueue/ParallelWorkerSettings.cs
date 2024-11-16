namespace UFG.BackgroundTaskQueue;

public class ParallelWorkerSettings
{
    private const int DefaultConcurrency = 25;

    private static TimeSpan DefaultStopTimeout => TimeSpan.FromSeconds(30);

    private static ParallelWorkerStopBehavior DefaultStopBehavior => ParallelWorkerStopBehavior.Drain;

    public int ConcurrentLimit { get; set; } = DefaultConcurrency;

    public ParallelWorkerStopBehavior StopBehavior { get; set; } = DefaultStopBehavior;

    public TimeSpan StopTimeout { get; set; } = DefaultStopTimeout;
}

public enum ParallelWorkerStopBehavior
{
    Drain = 0,
    CompleteInFlight = 1,
    Abandon = 2,
}
