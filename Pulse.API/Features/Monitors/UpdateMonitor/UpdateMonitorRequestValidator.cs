using FluentValidation;
using Pulse.API.Common.Validation;
using Pulse.BL.Common.Security.Ssrf;

namespace Pulse.API.Features.Monitors.UpdateMonitor;

public class UpdateMonitorRequestValidator : AbstractValidator<UpdateMonitorRequest>
{
    public UpdateMonitorRequestValidator(ISsrfGuard ssrfGuard)
    {
        RuleFor(x => x.Name).ApplyMonitorNameRules();
        RuleFor(x => x.Url).ApplyUrlRules(ssrfGuard);
        RuleFor(x => x.HttpMethod).ApplyHttpMethodRules();
        RuleFor(x => x.ResultPath).ApplyResultPathRules();
        RuleFor(x => x.Status).ApplyStatusRules();
        RuleFor(x => x.PollingIntervalSeconds).ApplyPollingIntervalRules();
        RuleFor(x => x.PollingTimeoutSeconds).ApplyPollingTimeoutRules();
    }
}
