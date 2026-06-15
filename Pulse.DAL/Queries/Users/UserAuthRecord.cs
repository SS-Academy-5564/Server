namespace Pulse.DAL.Queries.Users;

public sealed class UserAuthRecord
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
    public Guid OrganizationId { get; init; }
    public Guid RoleId { get; init; }
}
