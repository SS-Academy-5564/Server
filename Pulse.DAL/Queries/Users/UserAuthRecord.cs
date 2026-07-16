namespace Pulse.DAL.Queries.Users;

public sealed record UserAuthRecord(
    Guid Id,
    string Email,
    string PasswordHash,
    Guid OrganizationId,
    string RoleName,
    string OrganizationName);
