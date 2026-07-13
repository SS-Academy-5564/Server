namespace Pulse.BL.Features.Users.Members;

/// <summary>
/// Represents a paged list of organization members.
/// </summary>
public sealed record OrganizationMembersResult(int TotalCount, IReadOnlyList<OrganizationMemberResult> Members);
