using FluentValidation;

namespace Pulse.API.Features.Auth.EmailVerification;

/// <summary>
/// Validates email verification requests.
/// </summary>
public sealed class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
{
    /// <summary>
    /// Initializes validation rules for the verification token.
    /// </summary>
    public VerifyEmailRequestValidator()
    {
        RuleFor(request => request.Token)
            .NotEmpty().WithMessage("Verification token is required.")
            .MaximumLength(512).WithMessage("Verification token must not exceed 512 characters.");
    }
}
