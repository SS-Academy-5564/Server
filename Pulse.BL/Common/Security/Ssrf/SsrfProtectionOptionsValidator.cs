using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Extensions.Options;

namespace Pulse.BL.Common.Security.Ssrf;

/// <summary>
/// Validates SSRF protection options, ensuring every configured CIDR entry is
/// well-formed so misconfiguration fails fast at startup.
/// </summary>
public sealed class SsrfProtectionOptionsValidator : IValidateOptions<SsrfProtectionOptions>
{
    /// <summary>
    /// Validates the SSRF protection options, ensuring every configured CIDR entry is
    /// well-formed so misconfiguration fails fast at startup.
    /// </summary>
    /// <param name="name">The options instance name.</param>
    /// <param name="options">The options to validate.</param>
    /// <returns>A result indicating whether validation succeeded or failed.</returns>
    public ValidateOptionsResult Validate(string? name, SsrfProtectionOptions options)
    {
        List<string> errors = new();

        ValidateCidrs(options.AllowedCidrs, nameof(options.AllowedCidrs), errors);
        ValidateCidrs(options.BlockedCidrs, nameof(options.BlockedCidrs), errors);

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }

    /// <summary>
    /// Validates an array of CIDR entries, adding an error message for
    /// each entry that cannot be parsed as a valid IP network.
    /// </summary>
    /// <param name="cidrs">The CIDR strings to validate, or <c>null</c>.</param>
    /// <param name="propertyName">The name of the options property being validated, used in error messages.</param>
    /// <param name="errors">The list to which validation error messages are appended.</param>
    private static void ValidateCidrs(string[]? cidrs, string propertyName, List<string> errors)
    {
        if (cidrs is null)
        {
            return;
        }

        foreach (string cidr in cidrs)
        {
            string normalized = NormalizeCidr(cidr);
            if (!IPNetwork.TryParse(normalized, out _))
            {
                errors.Add($"{SsrfProtectionOptions.SectionName}:{propertyName} contains an invalid CIDR or IP: '{cidr}'.");
            }
        }
    }

    /// <summary>
    /// Normalizes a CIDR string by appending the default prefix length
    /// when the input is a bare IP address without a slash.
    /// </summary>
    /// <param name="cidr">The CIDR or IP address string to normalize.</param>
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
