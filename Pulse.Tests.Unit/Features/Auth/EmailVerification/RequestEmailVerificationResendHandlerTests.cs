using System.Data;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Pulse.BL.Features.Auth.EmailVerification;
using Pulse.BL.Features.Email;
using Pulse.DAL.Commands.EmailVerificationTokens;
using Pulse.DAL.Common.Repository;

namespace Pulse.Tests.Unit.Features.Auth.EmailVerification;

public class RequestEmailVerificationResendHandlerTests
{
    private const string Email = "user@example.com";
    private const string ReplacementToken = "replacement-token";
    private const string ReplacementTokenHash = "REPLACEMENT_HASH";

    private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactory = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IEmailVerificationTokenCommands> _tokenCommands = new();
    private readonly Mock<IEmailVerificationTokenService> _tokenService = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<ILogger<RequestEmailVerificationResendHandler>> _logger = new();
    private readonly DateTimeOffset _now = new(2026, 8, 10, 12, 0, 0, TimeSpan.Zero);
    private readonly RequestEmailVerificationResendHandler _handler;

    public RequestEmailVerificationResendHandlerTests()
    {
        _unitOfWork.Setup(unitOfWork => unitOfWork.DisposeAsync()).Returns(ValueTask.CompletedTask);
        _unitOfWorkFactory
            .Setup(factory => factory.CreateAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWork.Object);
        _tokenService.Setup(service => service.GenerateToken()).Returns(ReplacementToken);
        _tokenService.Setup(service => service.ComputeHash(ReplacementToken)).Returns(ReplacementTokenHash);
        _tokenCommands
            .Setup(commands => commands.PrepareResendByEmailAsync(
                It.IsAny<PrepareEmailVerificationResendByEmailInput>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Email);
        _emailService
            .Setup(service => service.SendEmailAsync(It.IsAny<SendEmailDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _handler = new RequestEmailVerificationResendHandler(
            _unitOfWorkFactory.Object,
            _tokenCommands.Object,
            _tokenService.Object,
            _emailService.Object,
            new FixedTimeProvider(_now),
            Options.Create(new EmailVerificationOptions
            {
                TokenLifetimeHours = 24,
                ResendCooldownSeconds = 60,
                VerificationPageUrl = "https://pulse.example.com/verify-email"
            }),
            _logger.Object);
    }

    [Fact]
    public async Task HandleAsync_UnverifiedAccount_ReplacesTokenAndSendsLocalizedEmail()
    {
        Result<ResendEmailVerificationResult> result = await _handler.HandleAsync(
            new RequestEmailVerificationResendCommand($" {Email} ", "uk"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ResendCooldownSeconds.Should().Be(60);
        _tokenCommands.Verify(commands => commands.PrepareResendByEmailAsync(
            It.Is<PrepareEmailVerificationResendByEmailInput>(input =>
                input.Email == Email &&
                input.ReplacementTokenHash == ReplacementTokenHash &&
                input.RequestedAt == _now &&
                input.ReplacementExpiresAt == _now.AddHours(24) &&
                input.ResendCooldownSeconds == 60),
            It.IsAny<CancellationToken>()), Times.Once);
        _emailService.Verify(service => service.SendEmailAsync(
            It.Is<SendEmailDto>(email =>
                email.To.Single() == Email &&
                email.Subject == "Підтвердьте адресу електронної пошти Pulse" &&
                email.HtmlBody != null &&
                email.HtmlBody.Contains(ReplacementToken) &&
                email.PlainTextBody != null &&
                email.PlainTextBody.Contains(ReplacementToken)),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_IneligibleOrMissingAccount_ReturnsGenericSuccessWithoutSending()
    {
        _tokenCommands
            .Setup(commands => commands.PrepareResendByEmailAsync(
                It.IsAny<PrepareEmailVerificationResendByEmailInput>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        Result<ResendEmailVerificationResult> result = await _handler.HandleAsync(
            new RequestEmailVerificationResendCommand(Email, "en"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ResendCooldownSeconds.Should().Be(60);
        _emailService.Verify(
            service => service.SendEmailAsync(It.IsAny<SendEmailDto>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_EmailDeliveryFails_ReturnsGenericSuccessWithoutCommittingOrLoggingToken()
    {
        _emailService
            .Setup(service => service.SendEmailAsync(It.IsAny<SendEmailDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Delivery failed"));

        Result<ResendEmailVerificationResult> result = await _handler.HandleAsync(
            new RequestEmailVerificationResendCommand(Email, "en"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _unitOfWork.Verify(unitOfWork => unitOfWork.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _logger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) =>
                    !state.ToString()!.Contains(ReplacementToken, StringComparison.Ordinal)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        /// <summary>
        /// Gets the fixed UTC time.
        /// </summary>
        /// <returns>The configured UTC time.</returns>
        public override DateTimeOffset GetUtcNow() => now;
    }
}
