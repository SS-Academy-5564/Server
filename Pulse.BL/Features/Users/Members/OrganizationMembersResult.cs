namespace Pulse.BL.Features.Users.Members;

/// <summary>
/// Represents the result of retrieving organization members.
/// </summary>
public sealed record OrganizationMembersResult(int TotalCount, IReadOnlyList<OrganizationMemberResult> Members);
