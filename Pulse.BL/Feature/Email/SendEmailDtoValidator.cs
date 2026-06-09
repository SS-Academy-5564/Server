using FluentValidation;

namespace Pulse.BL.Feature.Email;

public class SendEmailDtoValidator : AbstractValidator<SendEmailDto>
{
    public SendEmailDtoValidator()
    {
        RuleFor(x => x.To)
            .NotNull()
            .WithMessage("Recipients are required.")
            .Must(x => x.Any())
            .WithMessage("At least one recipient is required.");

        RuleForEach(x => x.To)
            .NotEmpty()
            .WithMessage("Recipient email addresses cannot be empty.")
            .EmailAddress()
            .WithMessage("'{PropertyValue}' is not a valid email address.");

        RuleFor(x => x.Subject)
            .NotEmpty()
            .WithMessage("Subject is required.")
            .MaximumLength(255)
            .WithMessage("Subject cannot exceed 255 characters.");

        RuleFor(x => x.HtmlBody)
            .MaximumLength(50000)
            .WithMessage("HTML body cannot exceed 50,000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.HtmlBody));

        RuleFor(x => x.PlainTextBody)
            .MaximumLength(50000)
            .WithMessage("Plain text body cannot exceed 50,000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.PlainTextBody));

        RuleFor(x => x)
            .Must(x =>
                !string.IsNullOrWhiteSpace(x.HtmlBody) ||
                !string.IsNullOrWhiteSpace(x.PlainTextBody))
            .WithMessage("Either HtmlBody or PlainTextBody must be provided.");

        When(x => x.ReplyTo is not null, () =>
        {
            RuleForEach(x => x.ReplyTo!)
                .NotEmpty()
                .WithMessage("Reply-To email addresses cannot be empty.")
                .EmailAddress()
                .WithMessage("'{PropertyValue}' is not a valid reply-to email address.");
        });
    }
}
