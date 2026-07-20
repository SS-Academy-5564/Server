using System.Diagnostics;
using FluentResults;
using Microsoft.Extensions.Logging;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security;
using Pulse.BL.Common.Security.Passwords;
using Pulse.BL.Common.Security.Tokens;
using Pulse.BL.Features.Auth.Login.LoginLockout;
using Pulse.DAL.Queries.Users;

namespace Pulse.BL.Features.Auth.Login;

/// <inheritdoc/>
public class LoginHandler : IAsyncHandler<LoginCommand, Result<LoginResult>>
{
    private readonly IUserQueries _userQueries;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILoginLockoutService _loginLockoutService;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        IUserQueries userQueries,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ILoginLockoutService loginLockoutService,
        ILogger<LoginHandler> logger)
    {
        _userQueries = userQueries;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _loginLockoutService = loginLockoutService;
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
        Stopwatch stopwatch = Stopwatch.StartNew();
        UserAuthRecord? user = await _userQueries.GetByEmailForAuthAsync(command.Email, ct);
        _logger.LogInformation($"GetByEmailForAuthAsync :  {stopwatch.ElapsedMilliseconds} ms");

        if (user is null)
        {
            LogFailure("user not found", command.Email);
            return Result.Fail(new UnauthorizedError("Invalid email or password."));
        }

        stopwatch.Restart();
        bool isAllowed = await _loginLockoutService.IsUserAllowedAsync(user.Id, ct);
        _logger.LogInformation($"IsUserAllowedAsync :  {stopwatch.ElapsedMilliseconds} ms");

        if (!isAllowed)
        {
            LogFailure("user not allowed", command.Email);
            return Result.Fail(new UnauthorizedError("Invalid email or password."));
        }

        stopwatch.Restart();
        bool passwordValid =
            _passwordHasher.VerifyHashedPassword(user.PasswordHash, command.Password);
        _logger.LogInformation($"VerifyHashedPassword :  {stopwatch.ElapsedMilliseconds} ms");

        if (!passwordValid)
        {
            stopwatch.Restart();
            await _loginLockoutService.AddFailedAttemptAsync(user.Id, ct);
            _logger.LogInformation($"AddFailedAttemptAsync :  {stopwatch.ElapsedMilliseconds} ms");

            LogFailure("invalid password", command.Email);
            return Result.Fail(new UnauthorizedError("Invalid email or password."));
        }

        stopwatch.Restart();
        await _loginLockoutService.ResetAttemptsAsync(user.Id, ct);
        _logger.LogInformation($"ResetAttemptsAsync :  {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        GeneratedJwtToken generatedToken =
            _jwtTokenGenerator.GenerateToken(user.Id, user.RoleName, user.OrganizationId, user.OrganizationName);
        _logger.LogInformation($"GenerateJwtToken :  {stopwatch.ElapsedMilliseconds} ms");

        return Result.Ok(new LoginResult(
            generatedToken.Token,
            generatedToken.ExpiresAt));
    }

    private void LogFailure(string reason, string email)
    {
        _logger.LogWarning(
            "Login failed: {Reason}. Identifier: {LoginIdentifier}",
            reason,
            PiiHasher.HashForLogging(email));
    }
}
