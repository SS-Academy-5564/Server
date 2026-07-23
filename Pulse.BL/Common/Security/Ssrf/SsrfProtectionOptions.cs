namespace Pulse.BL.Common.Security.Ssrf;

/// <summary>
/// Configures Server-Side Request Forgery (SSRF) protection for outbound
/// monitor polling requests.
/// </summary>
public sealed class SsrfProtectionOptions
{
    public const string SectionName = "SsrfProtection";

    /// <summary>
    /// When <c>true</c>, disables all built-in and configured deny ranges,
    /// permitting requests to private/internal destinations. Intended for
    /// environments that legitimately monitor internal hosts. Defaults to
    /// <c>false</c>.
    /// </summary>
    public bool AllowPrivateNetworks { get; init; }

    /// <summary>
    /// Explicit allowlist of CIDR ranges or single IP addresses that are
    /// permitted even when they fall inside a blocked range. Evaluated before
    /// any deny rule.
    /// </summary>
    public string[] AllowedCidrs { get; init; } = [];

    /// <summary>
    /// Additional CIDR ranges or single IP addresses to deny, layered on top of
    /// the built-in private/loopback/link-local/unique-local defaults.
    /// </summary>
    public string[] BlockedCidrs { get; init; } = [];
}
