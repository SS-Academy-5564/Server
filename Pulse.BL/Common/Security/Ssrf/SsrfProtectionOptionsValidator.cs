using Microsoft.Extensions.Options;

namespace Pulse.BL.Common.Security.Ssrf;

/// <summary>
/// Validates SSRF protection options, ensuring every configured CIDR entry is
/// well-formed so misconfiguration fails fast at startup.
/// </summary>
public sealed class SsrfProtectionOptionsValidator : IValidateOptions<SsrfProtectionOptions>
{
    public ValidateOptionsResult Validate(string? name, SsrfProtectionOptions options)
    {
        List<string> errors = new();

        ValidateCidrs(options.AllowedCidrs, nameof(options.AllowedCidrs), errors);
        ValidateCidrs(options.BlockedCidrs, nameof(options.BlockedCidrs), errors);

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }

    private static void ValidateCidrs(string[]? cidrs, string propertyName, List<string> errors)
    {
        if (cidrs is null)
        {
            return;
        }

        foreach (string cidr in cidrs)
        {
            if (!IpNetwork.TryParse(cidr, out _))
            {
                errors.Add($"{SsrfProtectionOptions.SectionName}:{propertyName} contains an invalid CIDR or IP: '{cidr}'.");
            }
        }
    }
}
