using FluentResults;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security.Tokens;
using Pulse.DAL.Commands.RefreshTokens;
using Pulse.DAL.Queries.RefreshTokens;

namespace Pulse.BL.Features.Auth.Logout;

/// <summary>
/// Handles the <see cref="LogoutCommand"/> to perform user logout.
/// </summary>
public class LogoutHandler : IAsyncHandler<LogoutCommand, Result>
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenQueries _refreshTokenQueries;
    private readonly IRefreshTokenCommands _refreshTokenCommands;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogoutHandler"/> class.
    /// </summary>
    /// <param name="refreshTokenService">Service for refresh token operations.</param>
    /// <param name="refreshTokenQueries">Queries for accessing refresh tokens.</param>
    /// <param name="refreshTokenCommands">Commands for modifying refresh tokens.</param>
    /// <param name="timeProvider">Provider for current time.</param>
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

    /// <summary>
    /// Handles the execution of the logout command by revoking the provided refresh token.
    /// </summary>
    /// <param name="command">The command containing the logout request data.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A successful result upon completion.</returns>
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
