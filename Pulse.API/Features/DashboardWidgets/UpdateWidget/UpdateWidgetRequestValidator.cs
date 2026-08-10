using FluentValidation;

namespace Pulse.API.Features.DashboardWidgets.UpdateWidget;

/// <summary>
/// Validates an <see cref="UpdateWidgetRequest"/> before it is processed.
/// </summary>
public class UpdateWidgetRequestValidator : AbstractValidator<UpdateWidgetRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateWidgetRequestValidator"/> class.
    /// </summary>
    public UpdateWidgetRequestValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Widget type is required.")
            .MaximumLength(50).WithMessage("Widget type must be at most 50 characters.");

        RuleFor(x => x.Title)
            .MaximumLength(100).WithMessage("Title must be at most 100 characters.");

        RuleFor(x => x.Subtitle)
            .MaximumLength(100).WithMessage("Subtitle must be at most 100 characters.");

        RuleFor(x => x.Metric)
            .NotEmpty().WithMessage("Metric is required.")
            .MaximumLength(100).WithMessage("Metric must be at most 100 characters.");

        RuleFor(x => x.TimeRange)
            .NotEmpty().WithMessage("Time range is required.")
            .MaximumLength(50).WithMessage("Time range must be at most 50 characters.");
    }
}
