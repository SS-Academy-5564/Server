using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace Pulse.BL.Features.Polling.ManualCheck.Queue;

public sealed class ManualCheckQueue : IManualCheckQueue
{
    private readonly Channel<ManualCheckJob> _channel;

    public ManualCheckQueue(IOptions<ManualCheckQueueOptions> options)
    {
        _channel = Channel.CreateBounded<ManualCheckJob>(new BoundedChannelOptions(options.Value.Capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
        });
    }

    public bool TryEnqueue(ManualCheckJob job) => _channel.Writer.TryWrite(job);

    public ValueTask<ManualCheckJob> DequeueAsync(CancellationToken ct) => _channel.Reader.ReadAsync(ct);
}
