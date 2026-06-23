namespace Pulse.BL.Features.Auth.Registration;

public record RegistrationCommand(string Email, string FirstName, string LastName, string Password);
