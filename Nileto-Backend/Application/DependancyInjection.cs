using System.Reflection;
using Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Application;

public static class DependancyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services.AddMessaging(typeof(DependancyInjection).Assembly);
    }

    private static IServiceCollection AddMessaging(this IServiceCollection services, Assembly assembly)
    {
        ServiceDescriptor[] commandServiceDescriptors = assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                           type.IsAssignableTo(typeof(ICommandHandler)))
            .Select(type => ServiceDescriptor.Transient(typeof(ICommandHandler), type))
            .ToArray();
        
        ServiceDescriptor[] queryServiceDescriptors = assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                           type.IsAssignableTo(typeof(IQueryHandler)))
            .Select(type => ServiceDescriptor.Transient(typeof(IQueryHandler), type))
            .ToArray();

        services.TryAddEnumerable(commandServiceDescriptors);
        services.TryAddEnumerable(queryServiceDescriptors);

        return services;
    }
}