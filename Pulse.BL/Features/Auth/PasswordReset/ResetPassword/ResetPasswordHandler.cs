using FluentResults;
using Microsoft.Extensions.Logging;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Security.Passwords;
using Pulse.BL.Common.Security.Tokens;
using Pulse.DAL.Commands.PasswordResetCodes;
using Pulse.DAL.Commands.Users;

namespace Pulse.BL.Features.Auth.PasswordReset.ResetPassword;

/// <inheritdoc/>
public class ResetPasswordHandler : IResetPasswordHandler
{
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserCommands _userCommands;
    private readonly IPasswordResetCodeCommands _codeCommands;
    private readonly ILogger<ResetPasswordHandler> _logger;

    public ResetPasswordHandler(
        IJwtTokenGenerator jwtTokenGenerator,
        IPasswordHasher passwordHasher,
        IUserCommands userCommands,
        IPasswordResetCodeCommands codeCommands,
        ILogger<ResetPasswordHandler> logger)
    {
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
        _userCommands = userCommands;
        _codeCommands = codeCommands;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result> ResetAsync(ResetPasswordCommand command, CancellationToken ct)
    {
        (Guid UserId, string Jti)? tokenData = await _jwtTokenGenerator.ValidatePasswordResetTokenAsync(command.ResetToken);

        if (tokenData is null)
        {
            _logger.LogWarning("Password reset attempted with an invalid or expired reset token.");
            return Result.Fail(new UnauthorizedError("The reset token is invalid or has expired."));
        }

        Guid userId = tokenData.Value.UserId;
        string jti = tokenData.Value.Jti;

        string newHash = _passwordHasher.HashPassword(command.NewPassword);

        bool success = await _userCommands.ConsumeResetTokenAndUpdatePasswordAsync(userId, jti, newHash, ct);

        if (!success)
        {
            _logger.LogWarning("Password reset failed for user {UserId}: Token already consumed, expired, or user not found.", userId);
            return Result.Fail(new UnauthorizedError("The reset token is invalid or has already been used."));
        }

        _logger.LogInformation("Password successfully reset for user {UserId}.", userId);

        return Result.Ok();
    }
}
