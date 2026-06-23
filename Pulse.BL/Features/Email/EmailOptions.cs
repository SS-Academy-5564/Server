namespace Pulse.BL.Features.Email;

public class EmailOptions
{
    public const string SectionName = "Email";

    public EmailProvider Provider { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}
