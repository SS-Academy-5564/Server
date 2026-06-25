using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Security;
using Pulse.BL.Common.Security.Passwords;
using Pulse.BL.Common.Security.Tokens;
using Pulse.DAL.Commands.PasswordResetCodes;
using Pulse.DAL.Queries.PasswordResetCodes;
using Pulse.DAL.Queries.Users;

namespace Pulse.BL.Features.Auth.PasswordReset.VerifyCode;

/// <inheritdoc/>
public class VerifyPasswordResetCodeHandler : IVerifyPasswordResetCodeHandler
{
    private readonly IUserQueries _userQueries;
    private readonly IPasswordResetCodeQueries _codeQueries;
    private readonly IPasswordResetCodeCommands _codeCommands;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly TimeProvider _timeProvider;
    private readonly PasswordResetOptions _options;
    private readonly ILogger<VerifyPasswordResetCodeHandler> _logger;

    public VerifyPasswordResetCodeHandler(
        IUserQueries userQueries,
        IPasswordResetCodeQueries codeQueries,
        IPasswordResetCodeCommands codeCommands,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        TimeProvider timeProvider,
        IOptions<PasswordResetOptions> options,
        ILogger<VerifyPasswordResetCodeHandler> logger)
    {
        _userQueries = userQueries;
        _codeQueries = codeQueries;
        _codeCommands = codeCommands;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _timeProvider = timeProvider;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<VerifyCodeResult>> VerifyAsync(
        VerifyPasswordResetCodeCommand command, CancellationToken ct)
    {
        Guid? userId = await _userQueries.GetIdByEmailAsync(command.Email, ct);

        if (userId is null)
        {
            return Result.Fail(new ValidationError("Invalid code."));
        }

        PasswordResetCodeRecord? record = await _codeQueries.GetActiveByUserIdAsync(userId.Value, ct);

        if (record is null)
        {
            LogFailure("no active code found", command.Email);
            return Result.Fail(new ValidationError("Invalid code."));
        }

        // Check expiry
        if (_timeProvider.GetUtcNow() > record.ExpiresAt)
        {
            await _codeCommands.DeleteByUserIdAsync(userId.Value, ct);
            LogFailure("code expired", command.Email);
            return Result.Fail(new ValidationError("The code has expired. Please request a new one."));
        }

        // Verify the plain code against the stored hash
        bool codeValid = _passwordHasher.VerifyHashedPassword(record.CodeHash, command.Code);

        if (!codeValid)
        {
            int failedAttempts = await _codeCommands.IncrementFailedAttemptsAsync(record.Id, ct);

            if (failedAttempts >= _options.MaxFailedAttempts)
            {
                await _codeCommands.DeleteByUserIdAsync(userId.Value, ct);
                LogFailure($"code invalidated after {failedAttempts} failed attempts", command.Email);
            }
            else
            {
                LogFailure($"wrong code ({failedAttempts}/{_options.MaxFailedAttempts} attempts)", command.Email);
            }

            return Result.Fail(new ValidationError("Invalid code."));
        }

        // Delete the used code
        await _codeCommands.DeleteByUserIdAsync(userId.Value, ct);

        string resetToken = _jwtTokenGenerator.GeneratePasswordResetToken(userId.Value, TimeSpan.FromMinutes(_options.ResetTokenLifetimeMinutes));

        _logger.LogInformation(
            "Password reset code verified successfully. Identifier: {Identifier}",
            PiiHasher.HashForLogging(command.Email));

        return Result.Ok(new VerifyCodeResult(resetToken));
    }

    private void LogFailure(string reason, string email)
    {
        _logger.LogWarning(
            "Password reset verification failed: {Reason}. Identifier: {Identifier}",
            reason,
            PiiHasher.HashForLogging(email));
    }
}
