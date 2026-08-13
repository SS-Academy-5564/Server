using FluentValidation;
using Pulse.API.Common.Validation;
using Pulse.BL.Common.Security.Ssrf;

namespace Pulse.API.Features.Monitors.CreateMonitor;

/// <summary>
/// Validates <see cref="CreateMonitorRequest"/> instances, ensuring monitor
/// configuration is well-formed and the endpoint URL does not target internal hosts.
/// </summary>
public class CreateMonitorRequestValidator : AbstractValidator<CreateMonitorRequest>
{
    public CreateMonitorRequestValidator(ISsrfGuard ssrfGuard)
    {
        RuleFor(x => x.Name).ApplyMonitorNameRules();
        RuleFor(x => x.Url).ApplyUrlRules(ssrfGuard);
        RuleFor(x => x.HttpMethod).ApplyHttpMethodRules();
        RuleFor(x => x.ResultPath).ApplyResultPathRules();
        RuleFor(x => x.PollingIntervalSeconds).ApplyPollingIntervalRules();
        RuleFor(x => x.PollingTimeoutSeconds).ApplyPollingTimeoutRules();
    }
}
