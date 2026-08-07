using FluentValidation;

namespace Pulse.API.Features.Auth.EmailVerification;

/// <summary>
/// Validates replacement email verification requests.
/// </summary>
public sealed class ResendEmailVerificationRequestValidator : AbstractValidator<ResendEmailVerificationRequest>
{
    /// <summary>
    /// Initializes validation rules for the expired verification token.
    /// </summary>
    public ResendEmailVerificationRequestValidator()
    {
        RuleFor(request => request.Token)
            .NotEmpty().WithMessage("Verification token is required.")
            .MaximumLength(512).WithMessage("Verification token must not exceed 512 characters.");
    }
}
