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
        Guid? userId = _jwtTokenGenerator.ValidatePasswordResetToken(command.ResetToken);

        if (userId is null)
        {
            _logger.LogWarning("Password reset attempted with an invalid or expired reset token.");
            return Result.Fail(new UnauthorizedError("The reset token is invalid or has expired."));
        }

        string newHash = _passwordHasher.HashPassword(command.NewPassword);
        await _userCommands.UpdatePasswordAsync(userId.Value, newHash, ct);

        // Defensive cleanup — codes should already be gone from the verify step
        await _codeCommands.DeleteByUserIdAsync(userId.Value, ct);

        _logger.LogInformation("Password successfully reset for user {UserId}.", userId.Value);

        return Result.Ok();
    }
}
