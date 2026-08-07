using Microsoft.Extensions.Options;

namespace Pulse.BL.Features.Auth.EmailVerification;

/// <summary>
/// Validates email verification configuration at application startup.
/// </summary>
public sealed class EmailVerificationOptionsValidator : IValidateOptions<EmailVerificationOptions>
{
    /// <summary>
    /// Validates the configured token lifetime and verification page URL.
    /// </summary>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="options">The options instance to validate.</param>
    /// <returns>A success result when the options are valid; otherwise, a failure result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public ValidateOptionsResult Validate(string? name, EmailVerificationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        List<string> errors = [];

        if (options.TokenLifetimeHours is < 1 or > 24)
        {
            errors.Add("EmailVerification:TokenLifetimeHours must be between 1 and 24.");
        }

        if (options.ResendCooldownSeconds is < 1 or > 3600)
        {
            errors.Add("EmailVerification:ResendCooldownSeconds must be between 1 and 3600.");
        }

        if (!Uri.TryCreate(options.VerificationPageUrl, UriKind.Absolute, out Uri? verificationUri))
        {
            errors.Add("EmailVerification:VerificationPageUrl must be an absolute URL.");
        }
        else if (verificationUri.Scheme != Uri.UriSchemeHttps &&
                 !(verificationUri.Scheme == Uri.UriSchemeHttp && verificationUri.IsLoopback))
        {
            errors.Add("EmailVerification:VerificationPageUrl must use HTTPS, except for loopback development URLs.");
        }

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }
}
