using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Extensions.Options;

namespace Pulse.BL.Common.Security.Ssrf;

/// <summary>
/// Default <see cref="ISsrfGuard"/> that blocks private, loopback, link-local,
/// unique-local and cloud-metadata destinations unless explicitly allowed.
/// </summary>
public sealed class SsrfGuard : ISsrfGuard
{
    private static readonly string[] DefaultBlockedCidrs =
    [
        "0.0.0.0/8",
        "127.0.0.0/8",
        "10.0.0.0/8",
        "172.16.0.0/12",
        "192.168.0.0/16",
        "169.254.0.0/16",
        "::/128",
        "::1/128",
        "fc00::/7",
        "fe80::/10",
    ];

    private readonly bool _allowPrivateNetworks;
    private readonly IReadOnlyList<IPNetwork> _allowed;
    private readonly IReadOnlyList<IPNetwork> _explicitlyBlocked;
    private readonly IReadOnlyList<IPNetwork> _defaultBlocked;

    /// <summary>
    /// Initializes a new instance of the <see cref="SsrfGuard"/> class.
    /// </summary>
    /// <param name="options">The SSRF protection options used to configure blocked and allowed CIDR ranges.</param>
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

        string trimmed = host.Trim().Trim('[', ']').TrimEnd('.');

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

    /// <summary>
    /// Normalizes an IP address by converting IPv4-mapped IPv6 addresses
    /// to their IPv4 representation; leaves all other addresses unchanged.
    /// </summary>
    /// <param name="address">The IP address to normalize.</param>
    /// <returns>The normalized IP address.</returns>
    private static IPAddress Normalize(IPAddress address)
        => address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address;

    /// <summary>
    /// Parses a collection of CIDR strings into <see cref="IPNetwork"/> instances.
    /// </summary>
    /// <param name="cidrs">The CIDR strings to parse.</param>
    /// <returns>A list of successfully parsed IP networks.</returns>
    private static List<IPNetwork> ParseNetworks(IEnumerable<string> cidrs)
    {
        List<IPNetwork> networks = new();

        foreach (string cidr in cidrs)
        {
            string normalized = NormalizeCidr(cidr);
            if (IPNetwork.TryParse(normalized, out IPNetwork network))
            {
                networks.Add(network);
            }
        }

        return networks;
    }

    /// <summary>
    /// Ensures a CIDR string includes a prefix length, appending the
    /// appropriate default (/32 for IPv4, /128 for IPv6) when absent.
    /// </summary>
    /// <param name="cidr">The CIDR string to normalize.</param>
    /// <returns>The normalized CIDR string with a prefix length.</returns>
    private static string NormalizeCidr(string cidr)
    {
        if (!cidr.Contains('/'))
        {
            if (IPAddress.TryParse(cidr, out IPAddress? address))
            {
                int prefixLength = address.AddressFamily == AddressFamily.InterNetwork ? 32 : 128;
                return $"{cidr}/{prefixLength}";
            }
        }

        return cidr;
    }
}
