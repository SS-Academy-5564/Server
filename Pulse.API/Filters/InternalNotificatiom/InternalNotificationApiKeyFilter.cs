using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Notifications;

namespace Pulse.API.Filters.InternalNotificatiom;

public sealed class InternalNotificationApiKeyFilter : IAsyncAuthorizationFilter
{
    private readonly InternalNotificationOptions _options;

    public InternalNotificationApiKeyFilter(IOptions<InternalNotificationOptions> options)
    {
        _options = options.Value;
    }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        string providedKey = context.HttpContext.Request.Headers[NotificationApiConstants.ApiKeyHeaderName].ToString();

        if (!IsValid(providedKey))
        {
            context.Result = new UnauthorizedResult();
        }

        return Task.CompletedTask;
    }

    private bool IsValid(string providedKey)
    {
        if (string.IsNullOrEmpty(providedKey))
        {
            return false;
        }

        byte[] expectedBytes = Encoding.UTF8.GetBytes(_options.ApiKey);
        byte[] providedBytes = Encoding.UTF8.GetBytes(providedKey);

        return expectedBytes.Length == providedBytes.Length &&
               CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }
}
