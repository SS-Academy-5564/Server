using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Pulse.API.Common.Notifications;
using Pulse.BL.Common.Notifications;

namespace Pulse.Tests.Unit.Common.Notifications;

public sealed class InternalNotificationApiKeyFilterTests
{
    [Fact]
    public async Task OnAuthorizationAsync_WithValidApiKey_AllowsRequest()
    {
        InternalNotificationApiKeyFilter filter = CreateFilter("secret");
        AuthorizationFilterContext context = CreateContext("secret");

        await filter.OnAuthorizationAsync(context);

        Assert.Null(context.Result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid")]
    public async Task OnAuthorizationAsync_WithInvalidApiKey_ReturnsUnauthorized(string? apiKey)
    {
        InternalNotificationApiKeyFilter filter = CreateFilter("secret");
        AuthorizationFilterContext context = CreateContext(apiKey);

        await filter.OnAuthorizationAsync(context);

        Assert.IsType<UnauthorizedResult>(context.Result);
    }

    private static InternalNotificationApiKeyFilter CreateFilter(string apiKey)
        => new(Options.Create(new InternalNotificationOptions { ApiKey = apiKey }));

    private static AuthorizationFilterContext CreateContext(string? apiKey)
    {
        DefaultHttpContext httpContext = new();
        if (apiKey is not null)
        {
            httpContext.Request.Headers[NotificationApiConstants.ApiKeyHeaderName] = apiKey;
        }

        ActionContext actionContext = new(
            httpContext,
            new RouteData(),
            new ActionDescriptor());

        return new AuthorizationFilterContext(actionContext, []);
    }
}
