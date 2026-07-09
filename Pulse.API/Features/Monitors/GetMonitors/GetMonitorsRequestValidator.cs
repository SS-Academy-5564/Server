using FluentValidation;
using Pulse.BL.Features.Monitors;

namespace Pulse.API.Features.Monitors.GetMonitors;

public class GetMonitorsRequestValidator : AbstractValidator<GetMonitorsRequest>
{
    public GetMonitorsRequestValidator()
    {
        RuleFor(x => x.Status)
            .Must(status => status is null || Enum.IsDefined(typeof(MonitorStatus), status.Value))
            .WithMessage("Status must be one of: Enabled, Disabled, Error.");
    }
}

