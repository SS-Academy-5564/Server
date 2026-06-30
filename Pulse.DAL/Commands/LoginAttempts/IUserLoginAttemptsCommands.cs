using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.LoginAttempts;

public interface IUserLoginAttemptsCommands : ICommands
{
    Task ResetAttemptsAsync(Guid userId, CancellationToken ct);

}
