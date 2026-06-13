using Microsoft.AspNetCore.Mvc;
using Pulse.API.Attributes;

namespace Pulse.API.Features.Example;

[AutoValidate]
public class ExampleController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> ExamplePost([Validate] ExampleRequest request)
    {
        return Ok();
    }
}
