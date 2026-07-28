using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Queries.RefreshTokens;

public interface IRefreshTokenQueries : IQueries
{
    Task<RefreshTokenRecord?> GetByTokenHashAsync(string tokenHash, CancellationToken ct);
}
