namespace Pulse.BL.Features.Users.Me;

public sealed record UserProfileResult(Guid Id, string Email, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
