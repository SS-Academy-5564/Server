using FluentValidation;
using Pulse.BL.Common.Pagination;

namespace Pulse.API.Features.Users.Members;

public sealed class GetOrganizationMembersRequestValidator
    : AbstractValidator<GetOrganizationMembersRequest>
{
    public GetOrganizationMembersRequestValidator()
    {
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
