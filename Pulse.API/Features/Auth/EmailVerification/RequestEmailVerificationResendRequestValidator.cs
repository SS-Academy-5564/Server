using FluentValidation;

namespace Pulse.API.Features.Auth.EmailVerification;

/// <summary>
/// Validates email-based verification resend requests.
/// </summary>
public sealed class RequestEmailVerificationResendRequestValidator
    : AbstractValidator<RequestEmailVerificationResendRequest>
{
    /// <summary>
    /// Initializes the email validation rules.
    /// </summary>
    public RequestEmailVerificationResendRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");
    }
}
