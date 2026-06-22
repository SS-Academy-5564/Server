using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.BL.Features.Email;

namespace Pulse.Tests.Unit.Feature.Email;

public class DummyEmailServiceTests
{
    [Fact]
    public async Task SendEmailAsync_ReturnsOkAsync()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<DummyEmailService>>();
        var service = new DummyEmailService(loggerMock.Object);

        var dto = new SendEmailDto(
            To: ["user@example.com"],
            Subject: "Test Subject",
            HtmlBody: "<p>Hello</p>",
            PlainTextBody: "Hello",
            ReplyTo: null);

        // Act
        Result result = await service.SendEmailAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
