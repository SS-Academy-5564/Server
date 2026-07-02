using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security;
using Pulse.BL.Common.Security.Passwords;
using Pulse.BL.Common.Security.Tokens;
using Pulse.DAL.Commands.PasswordResetCodes;
using Pulse.DAL.Queries.PasswordResetCodes;
using Pulse.DAL.Queries.Users;

namespace Pulse.BL.Features.Auth.PasswordReset.VerifyCode;

public class VerifyPasswordResetCodeHandler : IAsyncHandler<VerifyPasswordResetCodeCommand, Result<VerifyCodeResult>>
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

    /// <summary>
    /// Verifies the 6-digit OTP code for a password reset request.
    /// </summary>
    /// <param name="command">The command containing the email and code.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>
    /// A result containing a <see cref="VerifyCodeResult"/> with a short-lived reset token on success,
    /// or a failure result with an <c>InvalidCode</c> or <c>Timeout</c> error.
    /// </returns>
    public async Task<Result<VerifyCodeResult>> HandleAsync(
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

        if (await DeleteIfExpiredAsync(record, command.Email, ct))
        {
            return Result.Fail(new ValidationError("The code has expired. Please request a new one."));
        }

        if (!await VerifyCodeOrHandleFailureAsync(record, command.Code, command.Email, ct))
        {
            return Result.Fail(new ValidationError("Invalid code."));
        }

        // Generate a unique session ID (JTI) for the reset token
        string jti = Guid.NewGuid().ToString();

        // Atomically bind this JTI to the reset code record to prevent concurrent reuse
        // and to track token consumption during the final password reset step.
        // Update ExpiresAt to align with the JWT token lifetime.
        DateTimeOffset newExpiresAt = _timeProvider.GetUtcNow().AddMinutes(_options.ResetTokenLifetimeMinutes);
        bool marked = await _codeCommands.MarkAsVerifiedAsync(record.Id, jti, newExpiresAt, ct);
        if (!marked)
        {
            LogFailure("code already consumed concurrently", command.Email);
            return Result.Fail(new ValidationError("Invalid code."));
        }

        string resetToken = _jwtTokenGenerator.GeneratePasswordResetToken(userId.Value, jti, TimeSpan.FromMinutes(_options.ResetTokenLifetimeMinutes));

        _logger.LogInformation(
            "Password reset code verified successfully. Identifier: {Identifier}",
            PiiHasher.HashForLogging(command.Email));

        return Result.Ok(new VerifyCodeResult(resetToken));
    }

    private async Task<bool> DeleteIfExpiredAsync(PasswordResetCodeRecord record, string email, CancellationToken ct)
    {
        if (_timeProvider.GetUtcNow() >= record.ExpiresAt)
        {
            await _codeCommands.DeleteByIdAsync(record.Id, ct);
            LogFailure("code expired", email);
            return true;
        }

        return false;
    }

    private async Task<bool> VerifyCodeOrHandleFailureAsync(PasswordResetCodeRecord record, string code, string email, CancellationToken ct)
    {
        bool codeValid = _passwordHasher.VerifyHashedPassword(record.CodeHash, code);

        if (!codeValid)
        {
            int failedAttempts = await _codeCommands.IncrementFailedAttemptsAsync(record.Id, ct);

            if (failedAttempts >= _options.MaxFailedAttempts)
            {
                await _codeCommands.DeleteByIdAsync(record.Id, ct);
                LogFailure($"code invalidated after {failedAttempts} failed attempts", email);
            }
            else
            {
                LogFailure($"wrong code ({failedAttempts}/{_options.MaxFailedAttempts} attempts)", email);
            }

            return false;
        }

        return true;
    }

    private void LogFailure(string reason, string email)
    {
        _logger.LogWarning(
            "Password reset verification failed: {Reason}. Identifier: {Identifier}",
            reason,
            PiiHasher.HashForLogging(email));
    }
}
