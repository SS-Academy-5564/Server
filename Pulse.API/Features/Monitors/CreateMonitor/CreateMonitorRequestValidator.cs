using FluentValidation;
using Pulse.BL.Common.Security.Ssrf;

namespace Pulse.API.Features.Monitors.CreateMonitor;

public class CreateMonitorRequestValidator : AbstractValidator<CreateMonitorRequest>
{
    private const int MinPollingIntervalSeconds = 60;
    private const int MaxPollingIntervalSeconds = 24 * 60 * 60;
    private const int MinPollingTimeoutSeconds = 5;
    private const int MaxPollingTimeoutSeconds = 30;

    private static readonly string[] AllowedHttpMethods =
    [
        "GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS"
    ];

    private readonly ISsrfGuard _ssrfGuard;

    public CreateMonitorRequestValidator(ISsrfGuard ssrfGuard)
    {
        _ssrfGuard = ssrfGuard;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Monitor name is required.")
            .MaximumLength(64).WithMessage("Monitor name must be at most 64 characters.");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("Endpoint URL is required.")
            .MaximumLength(2083).WithMessage("Endpoint URL must be at most 2083 characters.")
            .Must(BeAValidHttpUrl).WithMessage("Endpoint URL must be a valid HTTP or HTTPS URL.")
            .Must(NotTargetInternalHost).WithMessage("Endpoint URL must not target a private or internal address.");

        RuleFor(x => x.HttpMethod)
            .NotEmpty().WithMessage("Request method is required.")
            .Must(method => AllowedHttpMethods.Contains(method, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Request method must be one of: {string.Join(", ", AllowedHttpMethods)}.");

        RuleFor(x => x.ResultPath)
            .NotEmpty().WithMessage("Result path is required.")
            .MaximumLength(255).WithMessage("Result path must be at most 255 characters.");

        RuleFor(x => x.PollingIntervalSeconds)
            .InclusiveBetween(MinPollingIntervalSeconds, MaxPollingIntervalSeconds)
            .WithMessage("Polling interval must be between 1 minute and 24 hours.");

        RuleFor(x => x.PollingTimeoutSeconds)
            .InclusiveBetween(MinPollingTimeoutSeconds, MaxPollingTimeoutSeconds)
            .WithMessage("Polling timeout must be between 5 and 30 seconds.");
    }

    private static bool BeAValidHttpUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private bool NotTargetInternalHost(string? url)
    {
        // Only enforce for well-formed HTTP(S) URLs; malformed URLs are caught by BeAValidHttpUrl.
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return true;
        }

        return _ssrfGuard.TryValidateHost(uri.Host, out _);
    }
}
