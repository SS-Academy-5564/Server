using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Errors;
using Resend;

namespace Pulse.BL.Features.Email;

public class ResendEmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly EmailOptions _options;
    private readonly ILogger<ResendEmailService> _logger;

    public ResendEmailService(
        IResend resend,
        IOptions<EmailOptions> options,
        ILogger<ResendEmailService> logger)
    {
        _resend = resend;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result> SendEmailAsync(
        SendEmailDto dto,
        CancellationToken cancellationToken = default)
    {
        var message = new EmailMessage
        {
            From = $"{_options.FromName} <{_options.FromAddress}>",
            Subject = dto.Subject,
            HtmlBody = dto.HtmlBody,
            TextBody = dto.PlainTextBody
        };

        foreach (string recipient in dto.To)
        {
            message.To.Add(recipient);
        }

        if (dto.ReplyTo is not null)
        {
            message.ReplyTo = EmailAddressList.From(dto.ReplyTo);
        }

        string recipients = string.Join(", ", dto.To);
        try
        {
            _logger.LogInformation(
                "Sending email via Resend. Recipients: {Recipients}, Subject: {Subject}",
                recipients,
                dto.Subject);
            await _resend.EmailSendAsync(message, cancellationToken);
            _logger.LogInformation(
                "Email sent successfully via Resend. Recipients: {Recipients}",
                recipients);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send email via Resend. Recipients: {Recipients}, Subject: {Subject}",
                recipients,
                dto.Subject);
            return Result.Fail(new InternalError("Failed to send email via Resend."));
        }
    }
}
