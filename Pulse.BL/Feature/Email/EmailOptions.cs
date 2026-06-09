namespace Pulse.BL.Feature.Email;

public class EmailOptions
{
    public const string SectionName = "Email";

    public string Provider { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}
