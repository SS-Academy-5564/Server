using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security;
using Pulse.BL.Common.Security.Passwords;
using Pulse.BL.Common.Security.Tokens;
using Pulse.BL.Features.Auth.Login.LoginLockout;
using Pulse.DAL.Commands.RefreshTokens;
using Pulse.DAL.Queries.RefreshTokens;
using Pulse.DAL.Queries.Users;

namespace Pulse.BL.Features.Auth.Login;

/// <inheritdoc/>
public class LoginHandler : IAsyncHandler<LoginCommand, Result<LoginResult>>
{
    private readonly IUserQueries _userQueries;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILoginLockoutService _loginLockoutService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenCommands _refreshTokenCommands;
    private readonly RefreshTokenOptions _refreshTokenOptions;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        IUserQueries userQueries,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ILoginLockoutService loginLockoutService,
        IRefreshTokenService refreshTokenService,
        IRefreshTokenCommands refreshTokenCommands,
        IOptions<RefreshTokenOptions> refreshTokenOptions,
        TimeProvider timeProvider,
        ILogger<LoginHandler> logger)
    {
        _userQueries = userQueries;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _loginLockoutService = loginLockoutService;
        _refreshTokenService = refreshTokenService;
        _refreshTokenCommands = refreshTokenCommands;
        _refreshTokenOptions = refreshTokenOptions.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user using provided credentials and returns a JWT token if successful.
    /// </summary>
    /// <param name="command">The login command containing email and password.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>
    /// A result containing <see cref="LoginResult"/> on success,
    /// or a failure result if authentication fails.
    /// </returns>
    public async Task<Result<LoginResult>> HandleAsync(LoginCommand command, CancellationToken ct)
    {
        UserAuthRecord? user = await _userQueries.GetByEmailForAuthAsync(command.Email, ct);

        if (user is null)
        {
            LogFailure("user not found", command.Email);
            return Result.Fail(new UnauthorizedError("Invalid email or password."));
        }

        if (user.IsLocked)
        {
            LogFailure("user not allowed", command.Email);
            return Result.Fail(new UnauthorizedError("Invalid email or password."));
        }

        bool passwordValid =
            _passwordHasher.VerifyHashedPassword(user.PasswordHash, command.Password);

        if (!passwordValid)
        {
            await _loginLockoutService.AddFailedAttemptAsync(user.Id, ct);
            LogFailure("invalid password", command.Email);
            return Result.Fail(new UnauthorizedError("Invalid email or password."));
        }

        if (user.FailedAttempts > 0)
        {
            await _loginLockoutService.ResetAttemptsAsync(user.Id, ct);
        }

        if (user.EmailVerifiedAt is null)
        {
            LogFailure("email not verified", command.Email);
            return Result.Fail(new EmailNotVerifiedError());
        }

        GeneratedJwtToken generatedToken =
            _jwtTokenGenerator.GenerateToken(user.Id, user.RoleName, user.OrganizationId, user.OrganizationName);

        string rawRefreshToken = _refreshTokenService.GenerateToken();
        string refreshTokenHash = _refreshTokenService.ComputeHash(rawRefreshToken);

        DateTimeOffset now = _timeProvider.GetUtcNow();
        RefreshTokenRecord refreshTokenRecord = new(
            Id: Guid.NewGuid(),
            UserId: user.Id,
            TokenHash: refreshTokenHash,
            FamilyId: Guid.NewGuid(),
            CreatedAt: now,
            ExpiresAt: now.AddDays(_refreshTokenOptions.ExpirationDays)
        );

        await _refreshTokenCommands.CreateAsync(refreshTokenRecord, ct);

        return Result.Ok(new LoginResult(
            generatedToken.Token,
            generatedToken.ExpiresAt,
            rawRefreshToken));
    }

    private void LogFailure(string reason, string email)
    {
        _logger.LogWarning(
            "Login failed: {Reason}. Identifier: {LoginIdentifier}",
            reason,
            PiiHasher.HashForLogging(email));
    }
}
