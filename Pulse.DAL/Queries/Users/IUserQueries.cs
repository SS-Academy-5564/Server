using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Queries.Users;

public interface IUserQueries : IQueries
{
    Task<bool> EmailExistsAsync(string email, CancellationToken ct);
    Task<UserAuthRecord?> GetByEmailForAuthAsync(string email, CancellationToken ct);
}
