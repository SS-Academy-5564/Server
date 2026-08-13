namespace Pulse.BL.Features.Auth.Registration;

/// <summary>
/// Contains the data required to register a user and localize the verification email.
/// </summary>
/// <param name="Email">The user's email address.</param>
/// <param name="FirstName">The user's first name.</param>
/// <param name="LastName">The user's last name.</param>
/// <param name="Password">The user's plain-text password to hash.</param>
/// <param name="Language">The preferred verification email language.</param>
public sealed record RegistrationCommand(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string Language);
