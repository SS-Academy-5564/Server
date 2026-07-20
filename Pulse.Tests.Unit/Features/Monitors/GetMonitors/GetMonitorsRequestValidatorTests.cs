using FluentValidation.TestHelper;
using Pulse.API.Features.Monitors.GetMonitors;
using Pulse.BL.Features.Monitors;

namespace Pulse.Tests.Unit.Features.Monitors.GetMonitors;

public class GetMonitorsRequestValidatorTests
{
    private readonly GetMonitorsRequestValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData(MonitorStatus.Enabled)]
    [InlineData(MonitorStatus.Disabled)]
    [InlineData(MonitorStatus.Error)]
    public void Validate_WithValidStatus_ShouldNotHaveValidationError(MonitorStatus? status)
    {
        TestValidationResult<GetMonitorsRequest> result = _validator.TestValidate(new GetMonitorsRequest(status, null, null));
        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Validate_WithInvalidStatus_ShouldHaveValidationError()
    {
        TestValidationResult<GetMonitorsRequest> result = _validator.TestValidate(new GetMonitorsRequest((MonitorStatus)999, null, null));
        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("Status must be one of: Enabled, Disabled, Error.");
    }

    [Fact]
    public void Validate_WithNonPositivePageNumber_ShouldHaveValidationError()
    {
        TestValidationResult<GetMonitorsRequest> result = _validator.TestValidate(new GetMonitorsRequest(null, 0, 10));

        result.ShouldHaveValidationErrorFor(x => x.PageNumber);
    }

    [Fact]
    public void Validate_WithLargePositivePageNumber_ShouldNotHaveValidationError()
    {
        TestValidationResult<GetMonitorsRequest> result = _validator.TestValidate(new GetMonitorsRequest(null, 101, 10));

        result.ShouldNotHaveValidationErrorFor(x => x.PageNumber);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_WithPageSizeOutsideAllowedRange_ShouldHaveValidationError(int pageSize)
    {
        TestValidationResult<GetMonitorsRequest> result = _validator.TestValidate(new GetMonitorsRequest(null, 1, pageSize));

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }
}
