using System.Net;
using System.Net.Sockets;

namespace Pulse.BL.Common.Security.Ssrf;

/// <summary>
/// Represents an IP network range (CIDR) and tests addresses for containment.
/// </summary>
internal sealed class IpNetwork
{
    private readonly byte[] _networkBytes;
    private readonly int _prefixLength;
    private readonly AddressFamily _family;

    private IpNetwork(byte[] networkBytes, int prefixLength, AddressFamily family)
    {
        _networkBytes = networkBytes;
        _prefixLength = prefixLength;
        _family = family;
    }

    /// <summary>
    /// Parses a CIDR string (e.g. <c>10.0.0.0/8</c>) or a single IP literal
    /// (treated as a full-length prefix).
    /// </summary>
    /// <returns><c>true</c> if parsing succeeded; otherwise <c>false</c>.</returns>
    public static bool TryParse(string? value, out IpNetwork? network)
    {
        network = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string[] parts = value.Split('/', 2);

        if (!IPAddress.TryParse(parts[0].Trim(), out IPAddress? address))
        {
            return false;
        }

        byte[] addressBytes = address.GetAddressBytes();
        int maxPrefix = addressBytes.Length * 8;

        int prefixLength;
        if (parts.Length == 1)
        {
            prefixLength = maxPrefix;
        }
        else if (!int.TryParse(parts[1].Trim(), out prefixLength) || prefixLength < 0 || prefixLength > maxPrefix)
        {
            return false;
        }

        network = new IpNetwork(addressBytes, prefixLength, address.AddressFamily);
        return true;
    }

    /// <summary>
    /// Determines whether the specified address falls within this network range.
    /// </summary>
    public bool Contains(IPAddress address)
    {
        if (address.AddressFamily != _family)
        {
            return false;
        }

        byte[] addressBytes = address.GetAddressBytes();
        if (addressBytes.Length != _networkBytes.Length)
        {
            return false;
        }

        int fullBytes = _prefixLength / 8;
        for (int i = 0; i < fullBytes; i++)
        {
            if (addressBytes[i] != _networkBytes[i])
            {
                return false;
            }
        }

        int remainingBits = _prefixLength % 8;
        if (remainingBits == 0)
        {
            return true;
        }

        int mask = (byte)(0xFF << (8 - remainingBits));
        return (addressBytes[fullBytes] & mask) == (_networkBytes[fullBytes] & mask);
    }
}
