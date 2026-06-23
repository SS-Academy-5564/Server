using FluentValidation;

namespace Pulse.API.Common.Validation;

public static class PasswordValidationRules
{
    public static IRuleBuilderOptions<T, string> MustContainUppercase<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.");
    }

    public static IRuleBuilderOptions<T, string> MustContainLowercase<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.");
    }

    public static IRuleBuilderOptions<T, string> MustContainDigit<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.Matches(@"[0-9]").WithMessage("Password must contain at least one digit.");
    }
}
