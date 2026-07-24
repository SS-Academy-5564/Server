using System.Threading.Channels;

namespace Pulse.BL.Common.BackgroundTasks;

public sealed class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<IServiceProvider, CancellationToken, Task>> _queue;

    public BackgroundTaskQueue(int capacity = 100)
    {
        var options = new BoundedChannelOptions(capacity) { FullMode = BoundedChannelFullMode.Wait };
        _queue = Channel.CreateBounded<Func<IServiceProvider, CancellationToken, Task>>(options);
    }
    /// <inheritdoc />
    public ValueTask EnqueueAsync(Func<IServiceProvider, CancellationToken, Task> workItem)
        => _queue.Writer.WriteAsync(workItem);

    /// <inheritdoc />
    public ValueTask<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken ct)
        => _queue.Reader.ReadAsync(ct);
}
