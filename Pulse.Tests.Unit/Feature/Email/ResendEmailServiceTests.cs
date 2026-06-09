using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Pulse.BL.Feature.Email;
using Resend;

namespace Pulse.Tests.Unit.Feature.Email;

public class ResendEmailServiceTests
{
    private static readonly EmailOptions DefaultOptions = new()
    {
        Provider = "resend",
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
    public async Task SendEmailAsync_WhenResendSucceeds_ReturnsOk()
    {
        // Arrange
        var resendMock = new Mock<IResend>();
        resendMock
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulResendResponse());

        var service = CreateService(resendMock);

        // Act
        var result = await service.SendEmailAsync(DefaultDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendEmailAsync_WhenResendThrows_ReturnsFail()
    {
        // Arrange
        var resendMock = new Mock<IResend>();
        resendMock
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Resend API error"));

        var service = CreateService(resendMock);

        // Act
        var result = await service.SendEmailAsync(DefaultDto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message)
            .Should().Contain("Failed to send email via Resend.");
    }

    [Fact]
    public async Task SendEmailAsync_WhenResendThrows_LogsError()
    {
        // Arrange
        var resendMock = new Mock<IResend>();
        resendMock
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Resend API error"));

        var loggerMock = new Mock<ILogger<ResendEmailService>>();
        var service = CreateService(resendMock, loggerMock);

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
    public async Task SendEmailAsync_MapsDtoAndOptions_ToEmailMessage()
    {
        // Arrange
        EmailMessage? capturedMessage = null;

        var resendMock = new Mock<IResend>();
        resendMock
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Callback<EmailMessage, CancellationToken>((message, _) => capturedMessage = message)
            .ReturnsAsync(SuccessfulResendResponse());

        var service = CreateService(resendMock);

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
    public async Task SendEmailAsync_PassesCancellationToken_ToResend()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        CancellationToken capturedToken = default;

        var resendMock = new Mock<IResend>();
        resendMock
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Callback<EmailMessage, CancellationToken>((_, token) => capturedToken = token)
            .ReturnsAsync(SuccessfulResendResponse());

        var service = CreateService(resendMock);

        // Act
        await service.SendEmailAsync(DefaultDto, cts.Token);

        // Assert
        capturedToken.Should().Be(cts.Token);
    }

    private static ResendResponse<Guid> SuccessfulResendResponse() =>
        new(Guid.NewGuid(), null!);

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
