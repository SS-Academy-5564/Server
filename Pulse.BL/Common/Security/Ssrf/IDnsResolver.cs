using System.Net;

namespace Pulse.BL.Common.Security.Ssrf;

/// <summary>
/// Resolves a host name to its IP addresses. Abstracted so connection-time SSRF
/// validation can be unit-tested without real DNS.
/// </summary>
public interface IDnsResolver
{
    Task<IPAddress[]> ResolveAsync(string host, CancellationToken ct);
}
