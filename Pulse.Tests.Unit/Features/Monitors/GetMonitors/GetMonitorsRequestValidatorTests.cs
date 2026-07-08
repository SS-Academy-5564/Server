using FluentValidation.TestHelper;
using Pulse.API.Features.Monitors.GetMonitors;
using Pulse.DAL.Queries.Monitors;

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
        TestValidationResult<GetMonitorsRequest> result = _validator.TestValidate(new GetMonitorsRequest(status));
        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Validate_WithInvalidStatus_ShouldHaveValidationError()
    {
        TestValidationResult<GetMonitorsRequest> result = _validator.TestValidate(new GetMonitorsRequest((MonitorStatus)999));
        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("Status must be one of: Enabled, Disabled, Error.");
    }
}
