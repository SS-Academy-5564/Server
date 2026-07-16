namespace Pulse.DAL.Queries.Members;

/// <summary>
/// Represents a member record returned from persistence.
/// </summary>
public sealed record MemberRecord(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string RoleName,
    DateTimeOffset JoinedAt);
