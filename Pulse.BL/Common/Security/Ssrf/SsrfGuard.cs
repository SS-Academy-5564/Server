using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Options;

namespace Pulse.BL.Common.Security.Ssrf;

/// <summary>
/// Default <see cref="ISsrfGuard"/> that blocks private, loopback, link-local,
/// unique-local and cloud-metadata destinations unless explicitly allowed.
/// </summary>
public sealed class SsrfGuard : ISsrfGuard
{
    // Built-in deny ranges covering the acceptance-criteria set.
    private static readonly string[] DefaultBlockedCidrs =
    [
        "0.0.0.0/8",        // "this host" / unspecified
        "127.0.0.0/8",      // loopback
        "10.0.0.0/8",       // private
        "172.16.0.0/12",    // private
        "192.168.0.0/16",   // private
        "169.254.0.0/16",   // link-local (incl. 169.254.169.254 metadata)
        "::/128",           // IPv6 unspecified (analog of 0.0.0.0)
        "::1/128",          // IPv6 loopback
        "fc00::/7",         // IPv6 unique-local
        "fe80::/10",        // IPv6 link-local
    ];

    private readonly bool _allowPrivateNetworks;
    private readonly IReadOnlyList<IpNetwork> _allowed;
    private readonly IReadOnlyList<IpNetwork> _explicitlyBlocked;
    private readonly IReadOnlyList<IpNetwork> _defaultBlocked;

    public SsrfGuard(IOptions<SsrfProtectionOptions> options)
    {
        SsrfProtectionOptions value = options.Value;
        _allowPrivateNetworks = value.AllowPrivateNetworks;
        _allowed = ParseNetworks(value.AllowedCidrs ?? []);
        _explicitlyBlocked = ParseNetworks(value.BlockedCidrs ?? []);
        _defaultBlocked = ParseNetworks(DefaultBlockedCidrs);
    }

    /// <inheritdoc />
    public bool IsAddressAllowed(IPAddress address)
    {
        IPAddress normalized = Normalize(address);

        // Only IPv4/IPv6 destinations are ever expected; reject anything else.
        if (normalized.AddressFamily is not (AddressFamily.InterNetwork or AddressFamily.InterNetworkV6))
        {
            return false;
        }

        if (_allowed.Any(network => network.Contains(normalized)))
        {
            return true;
        }

        // Explicitly configured deny ranges are always enforced, even when
        // private networks are otherwise permitted.
        if (_explicitlyBlocked.Any(network => network.Contains(normalized)))
        {
            return false;
        }

        if (_allowPrivateNetworks)
        {
            return true;
        }

        if (_defaultBlocked.Any(network => network.Contains(normalized)))
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    public bool TryValidateHost(string? host, out string? error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(host))
        {
            error = "Endpoint URL must include a host.";
            return false;
        }

        string trimmed = host.Trim().Trim('[', ']');

        if (IPAddress.TryParse(trimmed, out IPAddress? address))
        {
            if (!IsAddressAllowed(address))
            {
                error = "Endpoint URL must not target a private or internal address.";
                return false;
            }

            return true;
        }

        if (!_allowPrivateNetworks &&
            (trimmed.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
             trimmed.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase)))
        {
            error = "Endpoint URL must not target a private or internal address.";
            return false;
        }

        return true;
    }

    private static IPAddress Normalize(IPAddress address)
        => address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address;

    private static List<IpNetwork> ParseNetworks(IEnumerable<string> cidrs)
    {
        List<IpNetwork> networks = new();

        foreach (string cidr in cidrs)
        {
            if (IpNetwork.TryParse(cidr, out IpNetwork? network) && network is not null)
            {
                networks.Add(network);
            }
        }

        return networks;
    }
}
