using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Notifications;

namespace Pulse.API.Filters.InternalNotification;

/// <summary>
/// Authorization filter that validates incoming HTTP requests using an API key header.
/// </summary>
public sealed class InternalNotificationApiKeyFilter : IAsyncAuthorizationFilter
{
    private readonly InternalNotificationOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="InternalNotificationApiKeyFilter"/> class.
    /// </summary>
    /// <param name="options">The internal notification options containing the expected API key.</param>
    public InternalNotificationApiKeyFilter(IOptions<InternalNotificationOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Validates the API key header on incoming HTTP requests.
    /// </summary>
    /// <param name="context">The filter context for authorization.</param>
    /// <returns>A task that represents the asynchronous authorization operation.</returns>
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
