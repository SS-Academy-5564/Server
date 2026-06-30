namespace Pulse.BL.Common.Security;

public interface ICurrentUserService
{
    Guid? UserId { get; }
}
