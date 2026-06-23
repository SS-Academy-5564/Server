using Microsoft.Extensions.Options;

namespace Pulse.BL.Common.Security.Tokens;

/// <summary>
/// Validates JWT options values.
/// </summary>
public class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    /// <summary>
    /// Validates the JWT options instance.
    /// </summary>
    /// <param name="name">The options name.</param>
    /// <param name="options">The options to validate.</param>
    /// <returns>The validation result.</returns>
    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        List<string> errors = new();

        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            errors.Add("Jwt:Issuer is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            errors.Add("Jwt:Audience is required.");
        }

        if (string.IsNullOrWhiteSpace(options.SecretKey))
        {
            errors.Add("Jwt:SecretKey is required.");
        }
        else if (options.SecretKey.Length < 32)
        {
            errors.Add("Jwt:SecretKey must be at least 32 characters long.");
        }

        if (options.ExpirationMinutes <= 0)
        {
            errors.Add("Jwt:ExpirationMinutes must be greater than zero.");
        }

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
