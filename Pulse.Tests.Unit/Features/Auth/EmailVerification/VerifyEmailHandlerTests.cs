using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Pulse.BL.Common.Errors;
using Pulse.BL.Features.Auth.EmailVerification;
using Pulse.DAL.Commands.EmailVerificationTokens;

namespace Pulse.Tests.Unit.Features.Auth.EmailVerification;

public class VerifyEmailHandlerTests
{
    private readonly Mock<IEmailVerificationTokenCommands> _tokenCommands = new();
    private readonly Mock<IEmailVerificationTokenService> _tokenService = new();
    private readonly FakeTimeProvider _timeProvider = new(
        new DateTimeOffset(2026, 8, 4, 12, 0, 0, TimeSpan.Zero));
    private readonly VerifyEmailHandler _handler;

    public VerifyEmailHandlerTests()
    {
        _handler = new VerifyEmailHandler(
            _tokenCommands.Object,
            _tokenService.Object,
            _timeProvider);
    }

    [Fact]
    public async Task HandleAsync_ValidToken_ReturnsSuccessAndConsumesHash()
    {
        const string token = "raw-token";
        const string tokenHash = "TOKEN_HASH";
        _tokenService.Setup(s => s.ComputeHash(token)).Returns(tokenHash);
        _tokenCommands
            .Setup(c => c.ConsumeAsync(tokenHash, _timeProvider.GetUtcNow(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmailVerificationTokenConsumeResult.Succeeded);

        Result result = await _handler.HandleAsync(new VerifyEmailCommand(token), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _tokenCommands.Verify(
            c => c.ConsumeAsync(tokenHash, _timeProvider.GetUtcNow(), CancellationToken.None),
            Times.Once);
    }

    [Theory]
    [InlineData(EmailVerificationTokenConsumeResult.Invalid, typeof(InvalidEmailVerificationTokenError))]
    [InlineData(EmailVerificationTokenConsumeResult.Expired, typeof(ExpiredEmailVerificationTokenError))]
    [InlineData(EmailVerificationTokenConsumeResult.AlreadyUsed, typeof(AlreadyUsedEmailVerificationTokenError))]
    public async Task HandleAsync_NonConsumableToken_ReturnsDistinctError(
        EmailVerificationTokenConsumeResult consumeResult,
        Type expectedErrorType)
    {
        const string token = "raw-token";
        const string tokenHash = "TOKEN_HASH";
        _tokenService.Setup(s => s.ComputeHash(token)).Returns(tokenHash);
        _tokenCommands
            .Setup(c => c.ConsumeAsync(tokenHash, _timeProvider.GetUtcNow(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(consumeResult);

        Result result = await _handler.HandleAsync(new VerifyEmailCommand(token), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(error => error.GetType() == expectedErrorType);
    }
}
