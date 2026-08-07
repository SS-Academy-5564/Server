using FluentValidation;
using Pulse.BL.Features.Monitors;

namespace Pulse.API.Features.Monitors.UpdateMonitorStatus;

/// <summary>
/// Validates <see cref="UpdateMonitorStatusRequest"/> instances.
/// </summary>
public class UpdateMonitorStatusRequestValidator : AbstractValidator<UpdateMonitorStatusRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateMonitorStatusRequestValidator"/> class.
    /// </summary>
    public UpdateMonitorStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .Must(status => status == MonitorStatus.Enabled || status == MonitorStatus.Disabled)
            .WithMessage("Monitor status must be Enabled or Disabled.");
    }
}
