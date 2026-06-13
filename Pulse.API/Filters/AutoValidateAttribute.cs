using FluentValidation;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Pulse.API.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AutoValidateAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor is ControllerActionDescriptor descriptor)
        {
            foreach (var parameter in descriptor.MethodInfo.GetParameters())
            {
                if (!parameter.IsDefined(typeof(ValidateAttribute), inherit: true))
                    continue;

                if (!context.ActionArguments.TryGetValue(parameter.Name!, out var value) || value is null)
                    continue;

                var modelType = parameter.ParameterType;
                var validatorType = typeof(IValidator<>).MakeGenericType(modelType);
                if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
                    continue;

                var validationContext = new ValidationContext<object>(value);
                var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

                if (!result.IsValid)
                    throw new ValidationException(result.Errors);
            }
        }

        await next();
    }
}
