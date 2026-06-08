using FluentValidation;

namespace Pulse.API.Features.Example;

public class ExampleRequestValidator : AbstractValidator<ExampleRequest>
{
    public ExampleRequestValidator()
    {
        RuleFor(request => request.ExampleProperty)
            .MinimumLength(10)
            .MaximumLength(100);
    }
}
