using System.Reflection;
using Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependancyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISender, Sender>();

        return services.AddMessaging(typeof(DependancyInjection).Assembly);
    }

    private static IServiceCollection AddMessaging(this IServiceCollection services, Assembly assembly)
    {
        // Find all concrete types that implement ICommandHandler<,>
        var handlerTypes = assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false })
            .Select(type => new
            {
                ImplementationType = type,
                // Find all ICommandHandler<TCommand, TResponse> interfaces this type implements
                HandlerInterfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType &&
                                i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>))
                    .ToList()
            })
            .Where(x => x.HandlerInterfaces.Any())
            .ToList();

        // Register each handler for each ICommandHandler<,> interface it implements
        foreach (var handlerType in handlerTypes)
        {
            foreach (var handlerInterface in handlerType.HandlerInterfaces)
            {
                services.AddTransient(handlerInterface, handlerType.ImplementationType);
            }
        }

        return services;
    }
}