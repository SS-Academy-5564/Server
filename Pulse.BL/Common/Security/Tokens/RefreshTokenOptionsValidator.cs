using Microsoft.Extensions.Options;

namespace Pulse.BL.Common.Security.Tokens;

public class RefreshTokenOptionsValidator : IValidateOptions<RefreshTokenOptions>
{
    public ValidateOptionsResult Validate(string? name, RefreshTokenOptions options)
    {
        if (options.ExpirationDays is < 7 or > 30)
        {
            return ValidateOptionsResult.Fail("RefreshToken:ExpirationDays must be between 7 and 30.");
        }

        return ValidateOptionsResult.Success;
    }
}
