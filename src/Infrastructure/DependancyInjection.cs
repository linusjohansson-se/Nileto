using Application.Abstractions;
using Application.Abstractions.Messaging;
using Infrastructure.Database;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependancyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDatabase(configuration);
        services.AddServices();

        return services;
    }

    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Use DbContext pooling for better performance
        // Pool size of 128 handles most workloads well
        services.AddDbContextPool<ApplicationDbContext>(
            options => options
                .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                .UseSnakeCaseNamingConvention()
                // Replace the model cache key factory to support dynamic schema changes
                .ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>(),
            poolSize: 128);

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        // Register custom field management services
        services.AddScoped<CustomFieldService>();
        services.AddScoped<ICustomFieldService, CustomFieldServiceAdapter>();

        // Register helper for accessing custom field values
        services.AddScoped<CustomFieldAccessor>();

        return services;
    }
}