namespace Pulse.API.Features.Auth.Registration;

public record RegistrationRequest(string Email, string FirstName, string LastName, string Password);
