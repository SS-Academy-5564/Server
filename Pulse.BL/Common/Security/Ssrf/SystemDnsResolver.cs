using System.Net;

namespace Pulse.BL.Common.Security.Ssrf;

/// <summary>
/// <see cref="IDnsResolver"/> backed by the system resolver (<see cref="Dns"/>).
/// </summary>
public sealed class SystemDnsResolver : IDnsResolver
{
    public Task<IPAddress[]> ResolveAsync(string host, CancellationToken ct)
        => Dns.GetHostAddressesAsync(host, ct);
}
