using Microsoft.Extensions.Options;

namespace Pulse.BL.Features.Auth.PasswordReset;

public class PasswordResetOptionsValidator : IValidateOptions<PasswordResetOptions>
{
    public ValidateOptionsResult Validate(string? name, PasswordResetOptions options)
    {
        var failures = new List<string>();

        if (options.CodeTtlMinutes <= 0)
        {
            failures.Add("CodeTtlMinutes must be greater than zero.");
        }

        if (options.MaxFailedAttempts <= 0)
        {
            failures.Add("MaxFailedAttempts must be greater than zero.");
        }

        if (options.ResetTokenLifetimeMinutes <= 0)
        {
            failures.Add("ResetTokenLifetimeMinutes must be greater than zero.");
        }

        if (options.ResendCooldownSeconds <= 0)
        {
            failures.Add("ResendCooldownSeconds must be greater than zero.");
        }

        if (failures.Count > 0)
        {
            return ValidateOptionsResult.Fail(failures);
        }

        return ValidateOptionsResult.Success;
    }
}
