using System.Net;

namespace Pulse.BL.Common.Security.Ssrf;

/// <summary>
/// Decides whether an outbound request destination is permitted, protecting
/// against Server-Side Request Forgery (SSRF).
/// </summary>
public interface ISsrfGuard
{
    /// <summary>
    /// Determines whether a resolved destination IP address is allowed. This is
    /// the authoritative check, intended to run at connection time on every poll.
    /// </summary>
    bool IsAddressAllowed(IPAddress address);

    /// <summary>
    /// Best-effort create-time validation of a URL host for fast user feedback.
    /// IP literals are checked against <see cref="IsAddressAllowed"/>; obviously
    /// internal hostnames (e.g. <c>localhost</c>) are rejected. Hostnames are not
    /// resolved here — connection-time enforcement is authoritative.
    /// </summary>
    /// <param name="host">The URL host component.</param>
    /// <param name="error">A human-readable reason when the host is rejected.</param>
    /// <returns><c>true</c> if the host is acceptable; otherwise <c>false</c>.</returns>
    bool TryValidateHost(string? host, out string? error);
}
