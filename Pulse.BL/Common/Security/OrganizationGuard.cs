using FluentResults;
using Pulse.BL.Common.Errors;

namespace Pulse.BL.Common.Security;

/// <summary>
/// Guards that require the current user to belong to an organization.
/// </summary>
public static class OrganizationGuard
{
    /// <summary>
    /// Returns the current user's organization ID.
    /// </summary>
    /// <param name="user">The current user service.</param>
    /// <returns>The organization ID, or an <see cref="UnauthorizedError"/> when the user has none.</returns>
    public static Result<Guid> RequireOrganizationId(this ICurrentUserService user)
    {
        Guid? organizationId = user.OrganizationId;

        if (organizationId is null)
        {
            return Result.Fail(new UnauthorizedError("User is not associated with an organization."));
        }

        return Result.Ok(organizationId.Value);
    }
}
