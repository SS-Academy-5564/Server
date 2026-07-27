using FluentValidation.TestHelper;
using Pulse.API.Features.Monitors.UpdateMonitor;
using Pulse.BL.Features.Monitors;

namespace Pulse.Tests.Unit.Features.Monitors.UpdateMonitor;

public class UpdateMonitorRequestValidatorTests
{
    private readonly UpdateMonitorRequestValidator _validator = new();

    private static UpdateMonitorRequest ValidRequest()
        => new("EUR/USD Rate", "https://api.example.com/data", "GET", "data.usd.rate", MonitorStatus.Enabled, 300, 10);

    [Fact]
    public void Validate_ValidRequest_ShouldNotHaveValidationErrors()
    {
        TestValidationResult<UpdateMonitorRequest> result = _validator.TestValidate(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyName_ShouldHaveValidationError()
    {
        UpdateMonitorRequest request = ValidRequest() with { Name = "" };

        TestValidationResult<UpdateMonitorRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameTooLong_ShouldHaveValidationError()
    {
        UpdateMonitorRequest request = ValidRequest() with { Name = new string('a', 65) };

        TestValidationResult<UpdateMonitorRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com/data")]
    public void Validate_InvalidUrl_ShouldHaveValidationError(string url)
    {
        UpdateMonitorRequest request = ValidRequest() with { Url = url };

        TestValidationResult<UpdateMonitorRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Url);
    }

    [Theory]
    [InlineData("get")]
    [InlineData("POST")]
    [InlineData("HEAD")]
    public void Validate_AllowedHttpMethod_ShouldNotHaveMethodError(string method)
    {
        UpdateMonitorRequest request = ValidRequest() with { HttpMethod = method };

        TestValidationResult<UpdateMonitorRequest> result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.HttpMethod);
    }

    [Theory]
    [InlineData("")]
    [InlineData("TRACE")]
    public void Validate_UnsupportedHttpMethod_ShouldHaveValidationError(string method)
    {
        UpdateMonitorRequest request = ValidRequest() with { HttpMethod = method };

        TestValidationResult<UpdateMonitorRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.HttpMethod);
    }

    [Fact]
    public void Validate_EmptyResultPath_ShouldHaveValidationError()
    {
        UpdateMonitorRequest request = ValidRequest() with { ResultPath = "" };

        TestValidationResult<UpdateMonitorRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.ResultPath);
    }

    [Theory]
    [InlineData(MonitorStatus.Enabled)]
    [InlineData(MonitorStatus.Disabled)]
    public void Validate_AllowedStatus_ShouldNotHaveStatusError(MonitorStatus status)
    {
        UpdateMonitorRequest request = ValidRequest() with { Status = status };

        TestValidationResult<UpdateMonitorRequest> result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    [Theory]
    [InlineData(MonitorStatus.Error)]
    [InlineData((MonitorStatus)999)]
    public void Validate_DisallowedStatus_ShouldHaveValidationError(MonitorStatus status)
    {
        UpdateMonitorRequest request = ValidRequest() with { Status = status };

        TestValidationResult<UpdateMonitorRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Theory]
    [InlineData(59)]
    [InlineData(86_401)]
    public void Validate_PollingIntervalOutOfRange_ShouldHaveValidationError(int interval)
    {
        UpdateMonitorRequest request = ValidRequest() with { PollingIntervalSeconds = interval };

        TestValidationResult<UpdateMonitorRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PollingIntervalSeconds);
    }

    [Theory]
    [InlineData(60)]
    [InlineData(86_400)]
    public void Validate_PollingIntervalAtBounds_ShouldNotHaveIntervalError(int interval)
    {
        UpdateMonitorRequest request = ValidRequest() with { PollingIntervalSeconds = interval };

        TestValidationResult<UpdateMonitorRequest> result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.PollingIntervalSeconds);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(31)]
    public void Validate_PollingTimeoutOutOfRange_ShouldHaveValidationError(int timeout)
    {
        UpdateMonitorRequest request = ValidRequest() with { PollingTimeoutSeconds = timeout };

        TestValidationResult<UpdateMonitorRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PollingTimeoutSeconds);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(30)]
    public void Validate_PollingTimeoutAtBounds_ShouldNotHaveTimeoutError(int timeout)
    {
        UpdateMonitorRequest request = ValidRequest() with { PollingTimeoutSeconds = timeout };

        TestValidationResult<UpdateMonitorRequest> result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.PollingTimeoutSeconds);
    }
}
