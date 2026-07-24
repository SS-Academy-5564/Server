using Microsoft.Extensions.Options;

namespace Pulse.BL.Features.Polling.ManualTrigger.Queue;

public sealed class ManualCheckQueueOptionsValidator : IValidateOptions<ManualCheckQueueOptions>
{
    public ValidateOptionsResult Validate(string? name, ManualCheckQueueOptions options)
    {
        if (options.Capacity <= 0)
        {
            return ValidateOptionsResult.Fail($"{ManualCheckQueueOptions.SectionName}:Capacity must be greater than zero.");
        }

        return ValidateOptionsResult.Success;
    }
}
