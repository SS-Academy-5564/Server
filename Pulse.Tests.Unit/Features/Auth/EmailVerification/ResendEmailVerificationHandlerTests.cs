using System.Data;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Pulse.BL.Common.Errors;
using Pulse.BL.Features.Auth.EmailVerification;
using Pulse.BL.Features.Email;
using Pulse.DAL.Commands.EmailVerificationTokens;
using Pulse.DAL.Common.Repository;

namespace Pulse.Tests.Unit.Features.Auth.EmailVerification;

public class ResendEmailVerificationHandlerTests
{
    private const string PresentedToken = "expired-token";
    private const string PresentedTokenHash = "PRESENTED_HASH";
    private const string ReplacementToken = "replacement-token";
    private const string ReplacementTokenHash = "REPLACEMENT_HASH";
    private const string Recipient = "user@example.com";

    private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactory = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IEmailVerificationTokenCommands> _tokenCommands = new();
    private readonly Mock<IEmailVerificationTokenService> _tokenService = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<ILogger<ResendEmailVerificationHandler>> _logger = new();
    private readonly DateTimeOffset _now = new(2026, 8, 6, 12, 0, 0, TimeSpan.Zero);
    private readonly ResendEmailVerificationHandler _handler;

    public ResendEmailVerificationHandlerTests()
    {
        _unitOfWork.Setup(unitOfWork => unitOfWork.DisposeAsync()).Returns(ValueTask.CompletedTask);
        _unitOfWorkFactory
            .Setup(factory => factory.CreateAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWork.Object);
        _tokenService.Setup(service => service.ComputeHash(PresentedToken)).Returns(PresentedTokenHash);
        _tokenService.Setup(service => service.GenerateToken()).Returns(ReplacementToken);
        _tokenService.Setup(service => service.ComputeHash(ReplacementToken)).Returns(ReplacementTokenHash);
        _tokenCommands
            .Setup(commands => commands.PrepareResendAsync(
                It.IsAny<PrepareEmailVerificationTokenResendInput>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmailVerificationTokenResendPreparation(
                EmailVerificationTokenResendStatus.Succeeded,
                Recipient));
        _emailService
            .Setup(service => service.SendEmailAsync(It.IsAny<SendEmailDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _handler = new ResendEmailVerificationHandler(
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
    public async Task HandleAsync_ExpiredToken_StoresReplacementAndSendsVerificationEmail()
    {
        ResendEmailVerificationCommand command = new(PresentedToken, "en");

        Result<ResendEmailVerificationResult> result = await _handler.HandleAsync(
            command,
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ResendCooldownSeconds.Should().Be(60);
        _tokenCommands.Verify(commands => commands.PrepareResendAsync(
            It.Is<PrepareEmailVerificationTokenResendInput>(input =>
                input.PresentedTokenHash == PresentedTokenHash &&
                input.ReplacementTokenHash == ReplacementTokenHash &&
                input.RequestedAt == _now &&
                input.ReplacementExpiresAt == _now.AddHours(24) &&
                input.ResendCooldownSeconds == 60),
            It.IsAny<CancellationToken>()), Times.Once);
        _emailService.Verify(service => service.SendEmailAsync(
            It.Is<SendEmailDto>(email =>
                email.To.Single() == Recipient &&
                email.Subject == "Verify your Pulse email address" &&
                email.HtmlBody != null &&
                email.HtmlBody.Contains(ReplacementToken) &&
                email.PlainTextBody != null &&
                email.PlainTextBody.Contains(ReplacementToken)),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UkrainianLanguage_SendsUkrainianVerificationEmail()
    {
        Result<ResendEmailVerificationResult> result = await _handler.HandleAsync(
            new ResendEmailVerificationCommand(PresentedToken, "uk"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _emailService.Verify(service => service.SendEmailAsync(
            It.Is<SendEmailDto>(email =>
                email.To.Single() == Recipient &&
                email.Subject == "Підтвердьте адресу електронної пошти Pulse" &&
                email.HtmlBody != null &&
                email.HtmlBody.Contains("Підтвердити електронну адресу") &&
                email.HtmlBody.Contains("24 години") &&
                email.PlainTextBody != null &&
                email.PlainTextBody.Contains("24 години")),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(EmailVerificationTokenResendStatus.Invalid, typeof(ValidationError), AppError.Codes.EmailVerificationTokenInvalid)]
    [InlineData(EmailVerificationTokenResendStatus.NotExpired, typeof(ConflictError), AppError.Codes.EmailVerificationTokenNotExpired)]
    [InlineData(EmailVerificationTokenResendStatus.AlreadyUsed, typeof(ConflictError), AppError.Codes.EmailVerificationTokenAlreadyUsed)]
    [InlineData(EmailVerificationTokenResendStatus.Cooldown, typeof(TooManyRequestsError), AppError.Codes.TooManyRequests)]
    public async Task HandleAsync_IneligibleToken_ReturnsTypedFailureWithoutSending(
        EmailVerificationTokenResendStatus status,
        Type expectedErrorType,
        string expectedCode)
    {
        _tokenCommands
            .Setup(commands => commands.PrepareResendAsync(
                It.IsAny<PrepareEmailVerificationTokenResendInput>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmailVerificationTokenResendPreparation(status, null));

        Result<ResendEmailVerificationResult> result = await _handler.HandleAsync(
            new ResendEmailVerificationCommand(PresentedToken, "en"),
            CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        IError errorResult = result.Errors.Should().ContainSingle().Which;
        errorResult.Should().BeOfType(expectedErrorType);
        AppError error = errorResult.Should().BeAssignableTo<AppError>().Subject;
        error.Code.Should().Be(expectedCode);
        _emailService.Verify(
            service => service.SendEmailAsync(It.IsAny<SendEmailDto>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_EmailDeliveryFails_ReturnsInternalErrorWithoutCommittingOrLoggingTokens()
    {
        _emailService
            .Setup(service => service.SendEmailAsync(It.IsAny<SendEmailDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Delivery failed"));

        Result<ResendEmailVerificationResult> result = await _handler.HandleAsync(
            new ResendEmailVerificationCommand(PresentedToken, "en"),
            CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<InternalError>();
        _unitOfWork.Verify(unitOfWork => unitOfWork.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _logger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) =>
                    !state.ToString()!.Contains(PresentedToken, StringComparison.Ordinal) &&
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
