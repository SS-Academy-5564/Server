namespace Pulse.BL.Features.Users.Members;

/// <summary>
/// Represents a single member of an organization.
/// </summary>
public sealed record OrganizationMemberResult(
    Guid UserId,
    string Name,
    string Email,
    string Role,
    DateTimeOffset JoinedAt);
