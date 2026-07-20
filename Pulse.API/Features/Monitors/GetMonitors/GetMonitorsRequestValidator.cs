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

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .LessThan(100)
            .WithMessage("Page number must be greater than zero");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThan(100)
            .WithMessage("Page size must be greater than zero");
    }
}

