namespace Pulse.DAL.Queries.Users;

public sealed record UserProfileRecord(
    Guid Id,
    string Email,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
