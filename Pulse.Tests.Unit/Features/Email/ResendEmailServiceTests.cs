using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Pulse.BL.Features.Email;
using Resend;

namespace Pulse.Tests.Unit.Features.Email;

public class ResendEmailServiceTests
{
    private static readonly EmailOptions DefaultOptions = new()
    {
        Provider = EmailProvider.Resend,
        ApiKey = "re_test_key",
        FromAddress = "noreply@pulse.com",
        FromName = "Pulse"
    };

    private static readonly SendEmailDto DefaultDto = new(
        To: ["user@example.com"],
        Subject: "Test Subject",
        HtmlBody: "<p>Hello</p>",
        PlainTextBody: "Hello",
        ReplyTo: ["reply@example.com"]);

    [Fact]
    public async Task SendEmailAsync_WhenResendSucceeds_ReturnsOkAsync()
    {
        // Arrange
        var resendMock = new Mock<IResend>();
        resendMock
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulResendResponse());

        ResendEmailService service = CreateService(resendMock);

        // Act
        Result result = await service.SendEmailAsync(DefaultDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendEmailAsync_WhenResendThrows_ReturnsFailAsync()
    {
        // Arrange
        var resendMock = new Mock<IResend>();
        resendMock
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Resend API error"));

        ResendEmailService service = CreateService(resendMock);

        // Act
        Result result = await service.SendEmailAsync(DefaultDto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message)
            .Should().Contain("Failed to send email via Resend.");
    }

    [Fact]
    public async Task SendEmailAsync_WhenResendThrows_LogsErrorAsync()
    {
        // Arrange
        var resendMock = new Mock<IResend>();
        resendMock
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Resend API error"));

        var loggerMock = new Mock<ILogger<ResendEmailService>>();
        ResendEmailService service = CreateService(resendMock, loggerMock);

        // Act
        await service.SendEmailAsync(DefaultDto);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, _) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((_, _) => true)),
            Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_MapsDtoAndOptions_ToEmailMessageAsync()
    {
        // Arrange
        EmailMessage? capturedMessage = null;

        var resendMock = new Mock<IResend>();
        resendMock
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Callback<EmailMessage, CancellationToken>((message, _) => capturedMessage = message)
            .ReturnsAsync(SuccessfulResendResponse());

        ResendEmailService service = CreateService(resendMock);

        // Act
        await service.SendEmailAsync(DefaultDto);

        // Assert
        capturedMessage.Should().NotBeNull();
        capturedMessage!.From.ToString().Should().Be("Pulse <noreply@pulse.com>");
        capturedMessage.Subject.Should().Be("Test Subject");
        capturedMessage.HtmlBody.Should().Be("<p>Hello</p>");
        capturedMessage.TextBody.Should().Be("Hello");
        capturedMessage.To.Select(address => address.ToString())
            .Should().BeEquivalentTo(["user@example.com"]);
        capturedMessage.ReplyTo!.Select(address => address.ToString())
            .Should().BeEquivalentTo(["reply@example.com"]);
    }

    [Fact]
    public async Task SendEmailAsync_PassesCancellationToken_ToResendAsync()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        CancellationToken capturedToken = default;

        var resendMock = new Mock<IResend>();
        resendMock
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Callback<EmailMessage, CancellationToken>((_, token) => capturedToken = token)
            .ReturnsAsync(SuccessfulResendResponse());

        ResendEmailService service = CreateService(resendMock);

        // Act
        await service.SendEmailAsync(DefaultDto, cts.Token);

        // Assert
        capturedToken.Should().Be(cts.Token);
    }

    private static ResendResponse<Guid> SuccessfulResendResponse()
        => new(Guid.NewGuid(), null!);

    private static ResendEmailService CreateService(
        Mock<IResend> resendMock,
        Mock<ILogger<ResendEmailService>>? loggerMock = null)
    {
        loggerMock ??= new Mock<ILogger<ResendEmailService>>();

        return new ResendEmailService(
            resendMock.Object,
            Options.Create(DefaultOptions),
            loggerMock.Object);
    }
}
