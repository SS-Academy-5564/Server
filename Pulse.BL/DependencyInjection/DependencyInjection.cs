
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Pulse.BL.Common.Security;

namespace Pulse.BL.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services)
    {
        services.AddFromAssembly(Assembly.GetExecutingAssembly());
        services.AddTransient<IPasswordHasher, PasswordHasher>();

        return services;
    }
}
