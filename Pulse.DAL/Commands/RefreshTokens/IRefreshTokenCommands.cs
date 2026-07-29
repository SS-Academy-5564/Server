using Pulse.DAL.Common.Repository;
using Pulse.DAL.Queries.RefreshTokens;

namespace Pulse.DAL.Commands.RefreshTokens;

public interface IRefreshTokenCommands : ICommands
{
    Task CreateAsync(RefreshTokenRecord record, CancellationToken ct);
    Task UpdateAsync(RefreshTokenRecord record, CancellationToken ct);
    Task<bool> RotateAsync(RefreshTokenRecord oldRecord, RefreshTokenRecord newRecord, CancellationToken ct);
    Task RevokeFamilyAsync(Guid familyId, string reason, CancellationToken ct);
}
