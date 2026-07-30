namespace Pulse.API.Common;

internal static class RefreshTokenCookieFactory
{
    internal static CookieOptions Create(
        HttpRequest request,
        IHostEnvironment environment,
        DateTimeOffset? expires = null)
    {
        bool isSecureDeployment = !environment.IsDevelopment();

        return new CookieOptions
        {
            HttpOnly = true,
            Secure = isSecureDeployment || request.IsHttps,
            SameSite = isSecureDeployment ? SameSiteMode.None : SameSiteMode.Lax,
            Expires = expires
        };
    }
}
