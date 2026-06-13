using Microsoft.AspNetCore.Mvc;
using Pulse.API.Filters;

namespace Pulse.API.Attributes;

public class AutoValidateAttribute : TypeFilterAttribute
{
    public AutoValidateAttribute()
        : base(typeof(ValidateRequestActionFilter))
    {
    }
}
