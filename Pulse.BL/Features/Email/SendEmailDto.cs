namespace Pulse.BL.Features.Email;

public record SendEmailDto(
    IEnumerable<string> To,
    string Subject,
    string? HtmlBody,
    string? PlainTextBody,
    IEnumerable<string>? ReplyTo);
