namespace Pulse.BL.Feature.Email;

public record SendEmailDto(
    IEnumerable<string> To,
    string Subject,
    string? HtmlBody,
    string? PlainTextBody,
    IEnumerable<string>? ReplyTo);
