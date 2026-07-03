namespace Pulse.BL.Common.Security.CurrentUser;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string Role { get; }
    Guid? OrganizationId { get; }
}
