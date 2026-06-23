using FluentResults;
using Microsoft.Extensions.Logging;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Security;
using Pulse.BL.Common.Security.Passwords;
using Pulse.BL.Common.Security.Tokens;
using Pulse.DAL.Queries.Users;

namespace Pulse.BL.Features.Auth.Login;

/// <inheritdoc/>
public class LoginHandler : ILoginHandler
{
    private readonly IUserQueries _userQueries;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        IUserQueries userQueries,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<LoginHandler> logger)
    {
        _userQueries = userQueries;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<LoginResult>> LoginAsync(LoginCommand command, CancellationToken ct)
    {
        UserAuthRecord? user = await _userQueries.GetByEmailForAuthAsync(command.Email, ct);

        if (user is null)
        {
            LogFailure("user not found", command.Email);
            return Result.Fail(new UnauthorizedError("Invalid email or password."));
        }

        bool passwordValid =
            _passwordHasher.VerifyHashedPassword(user.PasswordHash, command.Password);

        if (!passwordValid)
        {
            LogFailure("invalid password", command.Email);
            return Result.Fail(new UnauthorizedError("Invalid email or password."));
        }

        GeneratedJwtToken generatedToken =
            _jwtTokenGenerator.GenerateToken(user.Id, user.RoleName, user.OrganizationId);

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