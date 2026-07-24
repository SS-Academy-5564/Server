using System.Threading.Channels;

namespace Pulse.BL.Features.Polling.ManualTrigger;

public sealed class ManualCheckQueue : IManualCheckQueue
{
    private readonly Channel<Guid> _channel;

    public ManualCheckQueue(int capacity)
    {
        _channel = Channel.CreateBounded<Guid>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
        });
    }

    public bool TryEnqueue(Guid monitorId) => _channel.Writer.TryWrite(monitorId);

    public ValueTask<Guid> DequeueAsync(CancellationToken ct) => _channel.Reader.ReadAsync(ct);
}
