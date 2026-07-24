using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace Pulse.BL.Features.Polling.ManualTrigger.Queue;

public sealed class ManualCheckQueue : IManualCheckQueue
{
    private readonly Channel<Guid> _channel;

    public ManualCheckQueue(IOptions<ManualCheckQueueOptions> options)
    {
        _channel = Channel.CreateBounded<Guid>(new BoundedChannelOptions(options.Value.Capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
        });
    }

    /// <inheritdoc/>
    public bool TryEnqueue(Guid monitorId) => _channel.Writer.TryWrite(monitorId);

    public ValueTask<Guid> DequeueAsync(CancellationToken ct) => _channel.Reader.ReadAsync(ct);
}
