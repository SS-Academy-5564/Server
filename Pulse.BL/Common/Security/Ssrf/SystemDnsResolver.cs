using System.Net;
using System.Net.Sockets;

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
    /// <exception cref="ArgumentNullException"><paramref name="host"/> is <see langword="null"/>.</exception>
    /// <exception cref="SocketException">A DNS resolution error occurred.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
    public Task<IPAddress[]> ResolveAsync(string host, CancellationToken ct)
        => Dns.GetHostAddressesAsync(host, ct);
}
