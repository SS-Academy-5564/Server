namespace Pulse.API.Features.Users.Members;

/// <summary>
/// Represents a request to retrieve a paginated list of organization members.
/// </summary>
/// <param name="PageNumber">The page number to retrieve (1-based).</param>
/// <param name="PageSize">The number of items to return per page.</param>
public sealed record GetOrganizationMembersRequest(int? PageNumber, int? PageSize);
