using FluentValidation.TestHelper;
using Microsoft.Extensions.Options;
using Pulse.API.Features.Monitors.CreateMonitor;
using Pulse.BL.Common.Security.Ssrf;

namespace Pulse.Tests.Unit.Features.Monitors.CreateMonitor;

public class CreateMonitorRequestValidatorTests
{
    private readonly CreateMonitorRequestValidator _validator =
        new(new SsrfGuard(Options.Create(new SsrfProtectionOptions())));

    private static CreateMonitorRequest ValidRequest()
        => new("EUR/USD Rate", "https://api.example.com/data", "GET", "data.usd.rate", 300, 10);

    [Fact]
    public void Validate_ValidRequest_ShouldNotHaveValidationErrors()
    {
        TestValidationResult<CreateMonitorRequest> result = _validator.TestValidate(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyName_ShouldHaveValidationError()
    {
        CreateMonitorRequest request = ValidRequest() with { Name = "" };

        TestValidationResult<CreateMonitorRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameTooLong_ShouldHaveValidationError()
    {
        CreateMonitorRequest request = ValidRequest() with { Name = new string('a', 65) };

        TestValidationResult<CreateMonitorRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com/data")]
    public void Validate_InvalidUrl_ShouldHaveValidationError(string url)
    {
        CreateMonitorRequest request = ValidRequest() with { Url = url };

        TestValidationResult<CreateMonitorRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Url);
    }

    [Theory]
    [InlineData("http://127.0.0.1/data")]
    [InlineData("http://169.254.169.254/latest/meta-data")]
    [InlineData("http://localhost:6379/data")]
    [InlineData("http://10.0.0.5/data")]
    [InlineData("http://[::1]/data")]
    [InlineData("http://[::]/data")]
    [InlineData("http://2130706433/data")]
    public void Validate_InternalHostUrl_ShouldHaveValidationError(string url)
    {
        CreateMonitorRequest request = ValidRequest() with { Url = url };

        TestValidationResult<CreateMonitorRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Url);
    }

    [Theory]
    [InlineData("https://api.example.com/data")]
    [InlineData("http://8.8.8.8/data")]
    public void Validate_PublicHostUrl_ShouldNotHaveUrlError(string url)
    {
        CreateMonitorRequest request = ValidRequest() with { Url = url };

        TestValidationResult<CreateMonitorRequest> result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Url);
    }

    [Theory]
    [InlineData("get")]
    [InlineData("POST")]
    [InlineData("HEAD")]
    public void Validate_AllowedHttpMethod_ShouldNotHaveMethodError(string method)
    {
        CreateMonitorRequest request = ValidRequest() with { HttpMethod = method };

        TestValidationResult<CreateMonitorRequest> result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.HttpMethod);
    }

    [Theory]
    [InlineData("")]
    [InlineData("TRACE")]
    public void Validate_UnsupportedHttpMethod_ShouldHaveValidationError(string method)
    {
        CreateMonitorRequest request = ValidRequest() with { HttpMethod = method };

        TestValidationResult<CreateMonitorRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.HttpMethod);
    }

    [Fact]
    public void Validate_EmptyResultPath_ShouldHaveValidationError()
    {
        CreateMonitorRequest request = ValidRequest() with { ResultPath = "" };

        TestValidationResult<CreateMonitorRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.ResultPath);
    }

    [Theory]
    [InlineData(59)]
    [InlineData(86_401)]
    public void Validate_PollingIntervalOutOfRange_ShouldHaveValidationError(int interval)
    {
        CreateMonitorRequest request = ValidRequest() with { PollingIntervalSeconds = interval };

        TestValidationResult<CreateMonitorRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PollingIntervalSeconds);
    }

    [Theory]
    [InlineData(60)]
    [InlineData(86_400)]
    public void Validate_PollingIntervalAtBounds_ShouldNotHaveIntervalError(int interval)
    {
        CreateMonitorRequest request = ValidRequest() with { PollingIntervalSeconds = interval };

        TestValidationResult<CreateMonitorRequest> result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.PollingIntervalSeconds);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(31)]
    public void Validate_PollingTimeoutOutOfRange_ShouldHaveValidationError(int timeout)
    {
        CreateMonitorRequest request = ValidRequest() with { PollingTimeoutSeconds = timeout };

        TestValidationResult<CreateMonitorRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PollingTimeoutSeconds);
    }
}
