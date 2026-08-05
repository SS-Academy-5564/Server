using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace Pulse.BL.Features.Polling.ManualCheck.Queue;

public sealed class ManualCheckQueue : IManualCheckQueue
{
    private readonly Channel<ManualCheckCommand> _channel;

    public ManualCheckQueue(IOptions<ManualCheckQueueOptions> options)
    {
        _channel = Channel.CreateBounded<ManualCheckCommand>(new BoundedChannelOptions(options.Value.Capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
        });
    }

    /// <inheritdoc/>
    public bool TryEnqueue(ManualCheckCommand command) => _channel.Writer.TryWrite(command);

    /// <inheritdoc/>
    public ValueTask<ManualCheckCommand> DequeueAsync(CancellationToken ct) => _channel.Reader.ReadAsync(ct);
}
