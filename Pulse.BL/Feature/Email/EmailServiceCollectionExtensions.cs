using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Resend;

namespace Pulse.BL.Feature.Email;

public static class EmailServiceCollectionExtensions
{
    public static IServiceCollection AddEmailing(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<EmailOptions>()
            .Bind(configuration.GetRequiredSection(EmailOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<EmailOptions>, EmailOptionsValidator>();

        string? emailProvider = configuration.GetValue<string>("Email:Provider");

        if (string.Equals(emailProvider, "dummy", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IEmailService, DummyEmailService>();
        }
        else if (string.Equals(emailProvider, "resend", StringComparison.OrdinalIgnoreCase))
        {
            services.AddHttpClient<ResendClient>();

            services.Configure<ResendClientOptions>(options => { options.ApiToken = configuration["Email:ApiKey"]!; });

            services.AddTransient<IResend, ResendClient>();
            services.AddScoped<IEmailService, ResendEmailService>();
        }
        else
        {
            throw new InvalidOperationException(
                $"Unsupported email provider '{emailProvider}'.");
        }

        return services;
    }
}
