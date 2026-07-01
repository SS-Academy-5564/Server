using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Queries.UserLoginAttempts;

/// <summary>
///  Defines queries for user login attempts look ups
/// </summary>
public interface IUserLoginAttemptsQueries : IQueries
{
    Task<UserLoginAttemptsRecord?> GetUserLoginAttemptsAsync(Guid userId, CancellationToken ct);
}
