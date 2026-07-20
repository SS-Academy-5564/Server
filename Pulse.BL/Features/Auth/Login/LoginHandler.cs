using System.Diagnostics;
using System.Text;
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
        List<(string Operation, long DurationMs)> timings = [];
        Stopwatch totalStopwatch = Stopwatch.StartNew();
        Stopwatch operationStopwatch = new();

        try
        {
            operationStopwatch.Start();
            UserAuthRecord? user = await _userQueries.GetByEmailForAuthAsync(command.Email, ct);
            AddTiming("User lookup");

            if (user is null)
            {
                LogFailure("user not found", command.Email);
                return Result.Fail(new UnauthorizedError("Invalid email or password."));
            }

            operationStopwatch.Restart();
            bool isAllowed = await _loginLockoutService.IsUserAllowedAsync(user.Id, ct);
            AddTiming("Lockout check");

            if (!isAllowed)
            {
                LogFailure("user not allowed", command.Email);
                return Result.Fail(new UnauthorizedError("Invalid email or password."));
            }

            operationStopwatch.Restart();
            bool passwordValid =
                _passwordHasher.VerifyHashedPassword(user.PasswordHash, command.Password);
            AddTiming("Password verification");

            if (!passwordValid)
            {
                operationStopwatch.Restart();
                await _loginLockoutService.AddFailedAttemptAsync(user.Id, ct);
                AddTiming("Add failed attempt");

                LogFailure("invalid password", command.Email);
                return Result.Fail(new UnauthorizedError("Invalid email or password."));
            }

            operationStopwatch.Restart();
            await _loginLockoutService.ResetAttemptsAsync(user.Id, ct);
            AddTiming("Reset attempts");

            operationStopwatch.Restart();
            GeneratedJwtToken generatedToken =
                _jwtTokenGenerator.GenerateToken(user.Id, user.RoleName, user.OrganizationId, user.OrganizationName);
            AddTiming("Generate JWT token");

            return Result.Ok(new LoginResult(
                generatedToken.Token,
                generatedToken.ExpiresAt));
        }
        finally
        {
            totalStopwatch.Stop();
            LogPerformanceTable(timings, totalStopwatch.ElapsedMilliseconds);
        }

        void AddTiming(string operation)
        {
            operationStopwatch.Stop();
            timings.Add((operation, operationStopwatch.ElapsedMilliseconds));
        }
    }

    private void LogPerformanceTable(
        IEnumerable<(string Operation, long DurationMs)> timings,
        long totalDurationMs)
    {
        StringBuilder table = new();
        table.AppendLine("| Operation             | Duration |");
        table.AppendLine("|-----------------------|----------|");

        foreach ((string operation, long durationMs) in timings)
        {
            table.AppendLine($"| {operation,-21} | {durationMs,6} ms |");
        }

        table.Append($"| {"Total",-21} | {totalDurationMs,6} ms |");

        _logger.LogInformation(
            "Login performance:{NewLine}{PerformanceTable}",
            Environment.NewLine,
            table.ToString());
    }

    private void LogFailure(string reason, string email)
    {
        _logger.LogWarning(
            "Login failed: {Reason}. Identifier: {LoginIdentifier}",
            reason,
            PiiHasher.HashForLogging(email));
    }
}
