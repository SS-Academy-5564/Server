namespace Pulse.DAL.Commands.Users;

/// <summary>
/// Describes the outcome of creating a user and contains the generated identifier on success.
/// </summary>
/// <param name="Status">The user creation outcome.</param>
/// <param name="UserId">The generated user identifier when creation succeeds.</param>
public sealed record CreateUserResult(CreateUserStatus Status, Guid? UserId);
