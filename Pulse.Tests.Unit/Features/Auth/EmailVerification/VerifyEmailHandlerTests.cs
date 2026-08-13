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
    [InlineData(
        EmailVerificationTokenConsumeResult.Invalid,
        typeof(ValidationError),
        AppError.Codes.EmailVerificationTokenInvalid)]
    [InlineData(
        EmailVerificationTokenConsumeResult.Expired,
        typeof(ValidationError),
        AppError.Codes.EmailVerificationTokenExpired)]
    [InlineData(
        EmailVerificationTokenConsumeResult.AlreadyUsed,
        typeof(ConflictError),
        AppError.Codes.EmailVerificationTokenAlreadyUsed)]
    public async Task HandleAsync_NonConsumableToken_ReturnsDistinctError(
        EmailVerificationTokenConsumeResult consumeResult,
        Type expectedErrorType,
        string expectedCode)
    {
        const string token = "raw-token";
        const string tokenHash = "TOKEN_HASH";
        _tokenService.Setup(s => s.ComputeHash(token)).Returns(tokenHash);
        _tokenCommands
            .Setup(c => c.ConsumeAsync(tokenHash, _timeProvider.GetUtcNow(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(consumeResult);

        Result result = await _handler.HandleAsync(new VerifyEmailCommand(token), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        IError error = result.Errors.Should().ContainSingle().Subject;
        error.GetType().Should().Be(expectedErrorType);
        error.Should().BeAssignableTo<AppError>().Subject.Code.Should().Be(expectedCode);
    }
}
