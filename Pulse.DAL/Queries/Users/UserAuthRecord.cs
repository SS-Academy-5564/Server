namespace Pulse.DAL.Queries.Users;

/// <summary>
/// Contains user and organization data required to issue authentication tokens.
/// </summary>
/// <param name="Id">The user's unique identifier.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="PasswordHash">The stored password hash.</param>
/// <param name="OrganizationId">The active organization's unique identifier.</param>
/// <param name="RoleName">The user's role name in the active organization.</param>
/// <param name="OrganizationName">The active organization's display name.</param>
/// <param name="FailedAttempts">The number of recorded failed login attempts.</param>
/// <param name="IsLocked">Indicates whether login is currently locked.</param>
/// <param name="EmailVerifiedAt">The UTC verification time, or <c>null</c> when the email is unverified.</param>
public sealed record UserAuthRecord(
    Guid Id,
    string Email,
    string PasswordHash,
    Guid OrganizationId,
    string RoleName,
    string OrganizationName,
    int FailedAttempts,
    bool IsLocked,
    DateTimeOffset? EmailVerifiedAt);
