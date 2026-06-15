using FluentResults;
using Microsoft.Extensions.Logging;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Security;
using Pulse.BL.Common.Security.Passwords;
using Pulse.BL.Common.Security.Tokens;
using Pulse.DAL.Queries.Users;

namespace Pulse.BL.Features.Auth.Login;

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

    public async Task<Result<LoginResult>> LoginAsync(LoginCommand command, CancellationToken ct)
    {
        var user = await _userQueries.GetByEmailForAuthAsync(command.Email, ct);
        if (user is null)
        {
            _logger.LogWarning(
                "Login failed: user not found. Identifier: {LoginIdentifier}",
                PiiHasher.HashForLogging(command.Email));
            return Result.Fail(new UnauthorizedError("Invalid email or password."));
        }

        var passwordValid = _passwordHasher.VerifyHashedPassword(user.PasswordHash, command.Password);
        if (!passwordValid)
        {
            _logger.LogWarning(
                "Login failed: invalid password. Identifier: {LoginIdentifier}",
                PiiHasher.HashForLogging(command.Email));
            return Result.Fail(new UnauthorizedError("Invalid email or password."));
        }

        var accessToken = _jwtTokenGenerator.GenerateToken(user.Id, user.RoleId, user.OrganizationId, out var expiresAt);

        var loginResult = new LoginResult
        {
            AccessToken = accessToken,
            ExpiresAt = expiresAt
        };

        return Result.Ok(loginResult);
    }
}
