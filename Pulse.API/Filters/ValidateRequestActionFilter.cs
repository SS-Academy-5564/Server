using FluentValidation;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Pulse.API.Extensions;

namespace Pulse.API.Filters;

public class ValidateRequestActionFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidateRequestActionFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Runs FluentValidation validators for all action parameters marked with <see cref="ValidateAttribute"/>.
    /// Throws <see cref="ValidationException"/> if any validator reports failures.
    /// </summary>
    /// <param name="context">The action execution context providing access to parameters and arguments.</param>
    /// <param name="next">The delegate to invoke the action when validation passes.</param>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var parameters = context.ActionDescriptor.Parameters
            .Select(parameter => parameter as ControllerParameterDescriptor)
            .Where(parameter => parameter is not null &&
                                parameter.ParameterInfo.IsDefined(typeof(ValidateAttribute), false));

        foreach (var parameter in parameters)
        {
            if (parameter is null)
            {
                continue;
            }

            if (!context.ActionArguments.TryGetValue(parameter.Name, out var parameterValue) ||
                parameterValue is null)
            {
                continue;
            }

            var validators = _serviceProvider.GetValidators(parameter.ParameterType);

            var validationContext = new ValidationContext<object>(parameterValue);

            var validationResults = await Task.WhenAll(
                validators.Select(validator =>
                    validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted)));

            var validationFailures = validationResults
                .Where(validationResult => !validationResult.IsValid)
                .SelectMany(r => r.Errors)
                .ToList();

            if (validationFailures.Count > 0)
            {
                throw new ValidationException(validationFailures);
            }
        }

        await next();
    }
}
