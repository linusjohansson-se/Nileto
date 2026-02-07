namespace WebApi.Startup;

public static class DependenciesConfig
{
    public static void AddDependencies(this IServiceCollection services)
    {
        services.AddOpenApiServices();
        services.AddEndpoints(typeof(Program).Assembly);
    }
}