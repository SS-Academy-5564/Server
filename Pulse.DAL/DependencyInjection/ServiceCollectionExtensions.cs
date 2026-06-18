using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        IEnumerable<(Type Interface, Type Implementation)> registrations = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract
                && (typeof(ICommands).IsAssignableFrom(t) || typeof(IQueries).IsAssignableFrom(t)))
            .SelectMany(t => t.GetInterfaces()
            .Where(i => i.Assembly == assembly && i != typeof(ICommands) && i != typeof(IQueries))
            .Select(i => (Interface: i, Implementation: t)));

        foreach ((Type? iface, Type? impl) in registrations)
        {
            services.AddScoped(iface, impl);
        }

        return services;
    }
}

