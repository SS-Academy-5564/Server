namespace Pulse.DAL.Queries.Members;

public sealed record UserOrganizationRecord(
    Guid OrganizationId,
    string OrganizationName,
    Guid RoleId,
    string RoleName,
    DateTimeOffset JoinedAt);
