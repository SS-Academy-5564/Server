using FluentValidation;

namespace Pulse.API.Features.Auth.PasswordReset;

public class VerifyPasswordResetCodeRequestValidator : AbstractValidator<VerifyPasswordResetCodeRequest>
{
    public VerifyPasswordResetCodeRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .Length(6).WithMessage("Code must be exactly 6 digits.")
            .Matches(@"^\d{6}$").WithMessage("Code must contain only digits.");
    }
}
