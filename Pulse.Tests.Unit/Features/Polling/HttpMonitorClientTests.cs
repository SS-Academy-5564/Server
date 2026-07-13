using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Pulse.BL.Features.Polling.Http;
using Pulse.DAL.Common.Constants;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.Tests.Unit.Features.Polling;

public class HttpMonitorClientTests
{
    private readonly Mock<HttpMessageHandler> _handler = new();
    private readonly Mock<IHttpClientFactory> _httpClientFactory = new();
    private readonly HttpMonitorClient _client;
    private string? _capturedClientName;

    public HttpMonitorClientTests()
    {
        var httpClient = new HttpClient(_handler.Object);

        _httpClientFactory
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Callback<string>(name => _capturedClientName = name)
            .Returns(httpClient);

        _client = new HttpMonitorClient(
            _httpClientFactory.Object,
            Mock.Of<ILogger<HttpMonitorClient>>());
    }

    [Fact]
    public async Task SendAsync_WhenResponseIsSuccessful_ReturnsSuccessAsync()
    {
        // Arrange
        const string expectedBody =
            """
            {
              "status": "ok"
            }
            """;

        HttpRequestMessage? capturedRequest = null;
        _handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(expectedBody)
            });

        MonitorRecord monitor = new(
            Guid.NewGuid(),
            "https://example.com/health",
            "GET",
            "status",
            60,
            30);

        // Act
        HttpMonitorResponse result = await _client.SendAsync(monitor, CancellationToken.None);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Get);
        capturedRequest.RequestUri.Should().Be(new Uri("https://example.com/health"));
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Body.Should().Be(expectedBody);
        result.RequestStatus.Should().Be(RequestStatusNames.Success);
        result.ResponseTimeMs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task SendAsync_WhenResponseIsNotSuccessful_ReturnsFailedAsync()
    {
        // Arrange
        const string expectedBody =
            """
            {
              "error": "server"
            }
            """;
        _handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(expectedBody)
            });

        MonitorRecord monitor = new(
            Guid.NewGuid(),
            "https://example.com/health",
            "GET",
            "status",
            60,
            30);

        // Act
        HttpMonitorResponse result = await _client.SendAsync(monitor, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(500);
        result.Body.Should().Be(expectedBody);
        result.RequestStatus.Should().Be(RequestStatusNames.Failed);
    }

    [Fact]
    public async Task SendAsync_WhenMethodIsUnsupported_ReturnsFailedWithoutSendingRequestAsync()
    {
        // Arrange
        _handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        MonitorRecord monitor = new(
            Guid.NewGuid(),
            "https://example.com/health",
            "fakeMethod",
            "status",
            60,
            30);

        // Act
        HttpMonitorResponse result = await _client.SendAsync(monitor, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().BeNull();
        result.RequestStatus.Should().Be(RequestStatusNames.Failed);
        _httpClientFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
        _handler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_WhenRequestTimesOut_ReturnsTimeoutAsync()
    {
        // Arrange
        _handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());

        MonitorRecord monitor = new(
            Guid.NewGuid(),
            "https://example.com/health",
            "GET",
            "status",
            60,
            30);

        // Act
        HttpMonitorResponse result = await _client.SendAsync(monitor, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().BeNull();
        result.RequestStatus.Should().Be(RequestStatusNames.Timeout);
    }

    [Fact]
    public async Task SendAsync_WhenHttpRequestExceptionIsThrown_ReturnsNetworkErrorAsync()
    {
        // Arrange
        _handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("DNS failure"));

        MonitorRecord monitor = new(
            Guid.NewGuid(),
            "https://example.com/health",
            "GET",
            "status",
            60,
            30);

        // Act
        HttpMonitorResponse result = await _client.SendAsync(monitor, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().BeNull();
        result.RequestStatus.Should().Be(RequestStatusNames.NetworkError);
    }

    [Fact]
    public async Task SendAsync_WhenParentTokenIsCanceled_ThrowsOperationCanceledExceptionAsync()
    {
        // Arrange
        _handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns<HttpRequestMessage, CancellationToken>(async (_, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10), ct);
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        MonitorRecord monitor = new(
            Guid.NewGuid(),
            "https://example.com/health",
            "GET",
            "status",
            60,
            30);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        Func<Task> act = () => _client.SendAsync(monitor, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task SendAsync_UsesNamedMonitorPollingClientAsync()
    {
        // Arrange
        _handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        MonitorRecord monitor = new(
            Guid.NewGuid(),
            "https://example.com/health",
            "GET",
            "status",
            60,
            30);

        // Act
        await _client.SendAsync(monitor, CancellationToken.None);

        // Assert
        _capturedClientName.Should().Be(HttpMonitorClient.ClientName);
    }
}
