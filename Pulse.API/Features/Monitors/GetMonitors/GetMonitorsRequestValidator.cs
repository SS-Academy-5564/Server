using FluentValidation;
using Pulse.BL.Common.Pagination;
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
            .InclusiveBetween(1, PaginationDefaults.MaxPageNumber)
            .When(x => x.PageNumber.HasValue)
            .WithMessage($"Page number must be between 1 and {PaginationDefaults.MaxPageNumber}");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PaginationDefaults.MaxPageSize)
            .When(x => x.PageSize.HasValue)
            .WithMessage($"Page size must be between 1 and {PaginationDefaults.MaxPageSize}.");
    }
}
