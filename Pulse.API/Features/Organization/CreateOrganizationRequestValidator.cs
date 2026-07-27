using FluentValidation;

namespace Pulse.API.Features.Organization;

public class CreateOrganizationRequestValidator : AbstractValidator<CreateOrganizationRequest>
{
    public CreateOrganizationRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Organization name is required")
            .MinimumLength(3).WithMessage("Min length is 3")
            .MaximumLength(50).WithMessage("Max length is 50");
    }
}
