namespace Pulse.BL.Features.Users.Me;

public sealed record UserOrganizationResult(
    Guid OrganizationId,
    string OrganizationName,
    Guid RoleId,
    string RoleName,
    DateTimeOffset JoinedAt);
