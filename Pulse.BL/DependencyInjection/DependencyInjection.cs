using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pulse.BL.Common.Security;
using Pulse.BL.Features.Email;

namespace Pulse.BL.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHandlersFromAssembly(Assembly.GetExecutingAssembly());
        services.AddTransient<IPasswordHasher, PasswordHasher>();
        services.AddEmailing(configuration);

        return services;
    }
}
