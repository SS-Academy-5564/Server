
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Pulse.BL.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var registrations = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Service"))
            .SelectMany(t => t.GetInterfaces()
            .Where(i => i.Assembly == assembly)
            .Select(i => (Interface: i, Implementation: t)));

        foreach (var (iface, impl) in registrations)
        {
            services.AddScoped(iface, impl);
        }

        return services;
    }
}
