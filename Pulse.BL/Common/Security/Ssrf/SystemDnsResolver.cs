using System.Net;

namespace Pulse.BL.Common.Security.Ssrf;

/// <summary>
/// <see cref="IDnsResolver"/> backed by the system resolver (<see cref="Dns"/>).
/// </summary>
public sealed class SystemDnsResolver : IDnsResolver
{
    /// <summary>
    /// Resolves a host name to its IP addresses using the system DNS resolver.
    /// </summary>
    /// <param name="host">The host name to resolve.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The resolved IP addresses.</returns>
    public Task<IPAddress[]> ResolveAsync(string host, CancellationToken ct)
        => Dns.GetHostAddressesAsync(host, ct);
}
