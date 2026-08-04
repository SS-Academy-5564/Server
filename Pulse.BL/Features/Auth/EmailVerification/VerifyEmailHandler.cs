using FluentResults;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.DAL.Commands.EmailVerificationTokens;

namespace Pulse.BL.Features.Auth.EmailVerification;

/// <summary>
/// Validates and consumes email verification tokens.
/// </summary>
public sealed class VerifyEmailHandler : IAsyncHandler<VerifyEmailCommand, Result>
{
    private readonly IEmailVerificationTokenCommands _tokenCommands;
    private readonly IEmailVerificationTokenService _tokenService;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="VerifyEmailHandler"/> class.
    /// </summary>
    /// <param name="tokenCommands">The token persistence operations.</param>
    /// <param name="tokenService">The service used to hash presented tokens.</param>
    /// <param name="timeProvider">The source of the current UTC time.</param>
    public VerifyEmailHandler(
        IEmailVerificationTokenCommands tokenCommands,
        IEmailVerificationTokenService tokenService,
        TimeProvider timeProvider)
    {
        _tokenCommands = tokenCommands;
        _tokenService = tokenService;
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Consumes a valid token once and reports a distinct failure for every invalid state.
    /// </summary>
    /// <param name="command">The command containing the raw verification token.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A successful result for a consumed token, or a typed token-state error.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the persistence layer returns an unsupported status.</exception>
    public async Task<Result> HandleAsync(VerifyEmailCommand command, CancellationToken ct)
    {
        string tokenHash = _tokenService.ComputeHash(command.Token);
        EmailVerificationTokenConsumeResult consumeResult = await _tokenCommands.ConsumeAsync(
            tokenHash,
            _timeProvider.GetUtcNow(),
            ct);

        return consumeResult switch
        {
            EmailVerificationTokenConsumeResult.Succeeded => Result.Ok(),
            EmailVerificationTokenConsumeResult.Invalid =>
                Result.Fail(new InvalidEmailVerificationTokenError()),
            EmailVerificationTokenConsumeResult.Expired =>
                Result.Fail(new ExpiredEmailVerificationTokenError()),
            EmailVerificationTokenConsumeResult.AlreadyUsed =>
                Result.Fail(new AlreadyUsedEmailVerificationTokenError()),
            _ => throw new InvalidOperationException(
                $"Unsupported email verification token status '{consumeResult}'.")
        };
    }
}
