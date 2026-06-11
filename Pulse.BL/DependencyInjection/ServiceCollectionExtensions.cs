
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Pulse.BL.Common.Handlers;

namespace Pulse.BL.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var registrations = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IAsyncHandler).IsAssignableFrom(t))
            .SelectMany(t => t.GetInterfaces()
            .Where(i => i.Assembly == assembly && i != typeof(IAsyncHandler) && i != typeof(IHandler))
            .Select(i => (Interface: i, Implementation: t)));

        foreach (var (iface, impl) in registrations)
        {
            services.AddScoped(iface, impl);
        }

        return services;
    }
}
