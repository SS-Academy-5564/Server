using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Security;
using Pulse.BL.Common.Security.Passwords;
using Pulse.BL.Features.Email;
using Pulse.DAL.Commands.PasswordResetCodes;
using Pulse.DAL.Queries.Users;

namespace Pulse.BL.Features.Auth.PasswordReset.RequestCode;

/// <inheritdoc/>
public class RequestPasswordResetHandler : IRequestPasswordResetHandler
{
    private readonly IUserQueries _userQueries;
    private readonly IPasswordResetCodeCommands _codeCommands;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly TimeProvider _timeProvider;
    private readonly PasswordResetOptions _options;
    private readonly ILogger<RequestPasswordResetHandler> _logger;

    public RequestPasswordResetHandler(
        IUserQueries userQueries,
        IPasswordResetCodeCommands codeCommands,
        IPasswordHasher passwordHasher,
        IEmailService emailService,
        TimeProvider timeProvider,
        IOptions<PasswordResetOptions> options,
        ILogger<RequestPasswordResetHandler> logger)
    {
        _userQueries = userQueries;
        _codeCommands = codeCommands;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _timeProvider = timeProvider;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result> RequestAsync(RequestPasswordResetCommand command, CancellationToken ct)
    {
        Guid? userId = await _userQueries.GetIdByEmailAsync(command.Email, ct);

        // Always return Ok to prevent email enumeration
        if (userId is null)
        {
            _logger.LogInformation(
                "Password reset requested for non-existent email. Identifier: {Identifier}",
                PiiHasher.HashForLogging(command.Email));
            return Result.Ok();
        }

        // Generate 6-digit OTP
        string plainCode = GenerateSixDigitCode();
        string codeHash = _passwordHasher.HashPassword(plainCode);
        DateTimeOffset expiresAt = _timeProvider.GetUtcNow().AddMinutes(_options.CodeTtlMinutes);

        // Delete any existing codes for this user, then create a fresh one
        await _codeCommands.DeleteByUserIdAsync(userId.Value, ct);
        await _codeCommands.CreateAsync(new PasswordResetCodeInput(userId.Value, codeHash, expiresAt), ct);

        // Send the email (best-effort; we still return Ok even if it fails)
        Result emailResult = await _emailService.SendEmailAsync(new SendEmailDto(
            To: [command.Email],
            Subject: PasswordResetEmailBuilder.BuildSubject(),
            HtmlBody: PasswordResetEmailBuilder.BuildHtmlBody(plainCode, _options.CodeTtlMinutes),
            PlainTextBody: PasswordResetEmailBuilder.BuildPlainTextBody(plainCode, _options.CodeTtlMinutes),
            ReplyTo: null), ct);

        if (emailResult.IsFailed)
        {
            _logger.LogWarning(
                "Password reset code created but email delivery failed for identifier: {Identifier}",
                PiiHasher.HashForLogging(command.Email));
        }

        _logger.LogInformation(
            "Password reset code issued. Identifier: {Identifier}",
            PiiHasher.HashForLogging(command.Email));

        return Result.Ok();
    }

    private static string GenerateSixDigitCode()
    {
        // Cryptographically random 6-digit code (000000–999999)
        int code = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, 1_000_000);
        return code.ToString("D6");
    }
}
