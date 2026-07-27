using System.Net;

namespace Pulse.BL.Common.Security.Ssrf;

/// <summary>
/// Resolves a host name to its IP addresses. Abstracted so connection-time SSRF
/// validation can be unit-tested without real DNS.
/// </summary>
public interface IDnsResolver
{
    /// <summary>
    /// Resolves a host name to its IP addresses.
    /// </summary>
    /// <param name="host">The host name to resolve.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The resolved IP addresses.</returns>
    Task<IPAddress[]> ResolveAsync(string host, CancellationToken ct);
}
