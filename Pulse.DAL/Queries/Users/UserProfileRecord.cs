namespace Pulse.DAL.Queries.Users;

public sealed record UserProfileRecord(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
