namespace Pulse.DAL.Commands.Users;

/// <summary>
/// Identifies the outcome of creating a user.
/// </summary>
public enum CreateUserStatus
{
    /// <summary>
    /// The user was created successfully.
    /// </summary>
    Succeeded = 0,

    /// <summary>
    /// A user with the same email already exists.
    /// </summary>
    DuplicateEmail = 1
}
