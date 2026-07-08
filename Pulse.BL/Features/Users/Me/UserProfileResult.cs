namespace Pulse.BL.Features.Users.Me;

public sealed record UserProfileResult(Guid Id, string Email, string FirstName, string LastName, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
