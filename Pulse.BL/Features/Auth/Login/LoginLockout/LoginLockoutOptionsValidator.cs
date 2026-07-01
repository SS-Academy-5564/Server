using Microsoft.Extensions.Options;

namespace Pulse.BL.Features.Auth.Login.LoginLockout;

/// <summary>
/// Validates account login lockout configuration.
/// </summary>
public sealed class LoginLockoutOptionsValidator : IValidateOptions<LoginLockoutOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, LoginLockoutOptions options)
    {
        List<string> errors = new();

        if (options.MaxFailedAttempts <= 0)
        {
            errors.Add($"{LoginLockoutOptions.SectionName}:MaxFailedAttempts must be greater than zero.");
        }

        if (options.LockoutDurationMinutes <= 0)
        {
            errors.Add($"{LoginLockoutOptions.SectionName}:LockoutDurationMinutes must be greater than zero.");
        }

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
