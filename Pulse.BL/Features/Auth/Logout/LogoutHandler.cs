using FluentResults;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security.Tokens;
using Pulse.DAL.Commands.RefreshTokens;
using Pulse.DAL.Queries.RefreshTokens;

namespace Pulse.BL.Features.Auth.Logout;

public class LogoutHandler : IAsyncHandler<LogoutCommand, Result>
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenQueries _refreshTokenQueries;
    private readonly IRefreshTokenCommands _refreshTokenCommands;
    private readonly TimeProvider _timeProvider;

    public LogoutHandler(
        IRefreshTokenService refreshTokenService,
        IRefreshTokenQueries refreshTokenQueries,
        IRefreshTokenCommands refreshTokenCommands,
        TimeProvider timeProvider)
    {
        _refreshTokenService = refreshTokenService;
        _refreshTokenQueries = refreshTokenQueries;
        _refreshTokenCommands = refreshTokenCommands;
        _timeProvider = timeProvider;
    }

    public async Task<Result> HandleAsync(LogoutCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return Result.Ok();
        }

        string tokenHash = _refreshTokenService.ComputeHash(command.RefreshToken);
        RefreshTokenRecord? currentRecord = await _refreshTokenQueries.GetByTokenHashAsync(tokenHash, ct);

        if (currentRecord is not null && currentRecord.RevokedAt is null)
        {
            RefreshTokenRecord updatedRecord = currentRecord with
            {
                RevokedAt = _timeProvider.GetUtcNow(),
                RevocationReason = "Logout"
            };

            await _refreshTokenCommands.UpdateAsync(updatedRecord, ct);
        }

        return Result.Ok();
    }
}
