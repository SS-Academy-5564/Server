using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Security.Passwords;
using Pulse.BL.Common.Security.Tokens;
using Pulse.BL.Feature.Email;

namespace Pulse.BL.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFromAssembly(Assembly.GetExecutingAssembly());
        services.AddSingleton(TimeProvider.System);
        services.AddTransient<IPasswordHasher, PasswordHasher>();
        services.AddTransient<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();
        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetRequiredSection(JwtOptions.SectionName))
            .ValidateOnStart();
        services.AddEmailing(configuration);

        return services;
    }
}
