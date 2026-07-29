using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security.Tokens;
using Pulse.BL.Features.Auth.Login;
using Pulse.DAL.Commands.RefreshTokens;
using Pulse.DAL.Queries.RefreshTokens;
using Pulse.DAL.Queries.Users;

namespace Pulse.BL.Features.Auth.Refresh;

public class RefreshHandler : IAsyncHandler<RefreshCommand, Result<LoginResult>>
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenQueries _refreshTokenQueries;
    private readonly IRefreshTokenCommands _refreshTokenCommands;
    private readonly IUserQueries _userQueries;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly RefreshTokenOptions _refreshTokenOptions;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<RefreshHandler> _logger;

    public RefreshHandler(
        IRefreshTokenService refreshTokenService,
        IRefreshTokenQueries refreshTokenQueries,
        IRefreshTokenCommands refreshTokenCommands,
        IUserQueries userQueries,
        IJwtTokenGenerator jwtTokenGenerator,
        IOptions<RefreshTokenOptions> refreshTokenOptions,
        TimeProvider timeProvider,
        ILogger<RefreshHandler> logger)
    {
        _refreshTokenService = refreshTokenService;
        _refreshTokenQueries = refreshTokenQueries;
        _refreshTokenCommands = refreshTokenCommands;
        _userQueries = userQueries;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenOptions = refreshTokenOptions.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<Result<LoginResult>> HandleAsync(RefreshCommand command, CancellationToken ct)
    {
        string tokenHash = _refreshTokenService.ComputeHash(command.RefreshToken);
        RefreshTokenRecord? currentRecord = await _refreshTokenQueries.GetByTokenHashAsync(tokenHash, ct);

        DateTimeOffset now = _timeProvider.GetUtcNow();

        if (currentRecord is null)
        {
            _logger.LogWarning("Refresh failed: Token not found.");
            return Result.Fail(new UnauthorizedError("Invalid refresh token."));
        }

        if (currentRecord.RevokedAt is not null || currentRecord.ExpiresAt <= now)
        {
            _logger.LogWarning("Refresh failed: Token revoked or expired.");
            return Result.Fail(new UnauthorizedError("Invalid refresh token."));
        }

        if (currentRecord.UsedAt is not null)
        {
            _logger.LogWarning("Refresh token reuse detected for FamilyId: {FamilyId}. Revoking entire family.", currentRecord.FamilyId);
            await _refreshTokenCommands.RevokeFamilyAsync(currentRecord.FamilyId, "RefreshTokenReuse", ct);
            return Result.Fail(new UnauthorizedError("Invalid refresh token."));
        }

        UserAuthRecord? user = await _userQueries.GetByIdForAuthAsync(currentRecord.UserId, ct);

        if (user is null || user.IsLocked)
        {
            _logger.LogWarning("Refresh failed: User not found or locked.");
            return Result.Fail(new UnauthorizedError("Invalid user state."));
        }

        (RefreshTokenRecord? newRecord, string? newRawRefreshToken) = await RotateRefreshTokenAsync(currentRecord, user.Id, now, ct);

        GeneratedJwtToken generatedToken =
            _jwtTokenGenerator.GenerateToken(user.Id, user.RoleName, user.OrganizationId, user.OrganizationName);

        return Result.Ok(new LoginResult(
            generatedToken.Token,
            generatedToken.ExpiresAt,
            newRawRefreshToken));
    }

    private async Task<(RefreshTokenRecord, string)> RotateRefreshTokenAsync(
        RefreshTokenRecord currentRecord, Guid userId, DateTimeOffset now, CancellationToken ct)
    {
        string newRawRefreshToken = _refreshTokenService.GenerateToken();
        string newRefreshTokenHash = _refreshTokenService.ComputeHash(newRawRefreshToken);

        RefreshTokenRecord newRecord = new(
            Id: Guid.NewGuid(),
            UserId: userId,
            TokenHash: newRefreshTokenHash,
            FamilyId: currentRecord.FamilyId,
            CreatedAt: now,
            ExpiresAt: now.AddDays(_refreshTokenOptions.ExpirationDays)
        );

        RefreshTokenRecord updatedCurrentRecord = currentRecord with
        {
            UsedAt = now,
            ReplacedByTokenId = newRecord.Id
        };

        await _refreshTokenCommands.CreateAsync(newRecord, ct);
        await _refreshTokenCommands.UpdateAsync(updatedCurrentRecord, ct);

        return (newRecord, newRawRefreshToken);
    }
}
