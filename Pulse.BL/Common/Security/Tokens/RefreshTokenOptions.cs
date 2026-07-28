namespace Pulse.BL.Common.Security.Tokens;

public class RefreshTokenOptions
{
    public const string SectionName = "RefreshToken";

    public int ExpirationDays { get; set; } = 14;
}
