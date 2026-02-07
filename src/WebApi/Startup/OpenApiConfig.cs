using Scalar.AspNetCore;

namespace WebApi.Startup;

public static class OpenApiConfig
{
    public static void AddOpenApiServices(this IServiceCollection services)
    {
        services.AddOpenApi();
    }

    public static void UseOpenApi(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.Title = "Nileto API";
                options.Theme = ScalarTheme.DeepSpace;
                options.Layout = ScalarLayout.Modern;
                options.DarkMode = true;
                options.HideDarkModeToggle = true;
                options.HideClientButton();
            });
        }
        else
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.Title = "Nileto API";
                options.Theme = ScalarTheme.DeepSpace;
                options.Layout = ScalarLayout.Modern;
                options.DarkMode = true;
                options.HideDarkModeToggle = true;
                options.HideClientButton();
                options.HideDeveloperTools();
                options.ShowDeveloperTools = DeveloperToolsVisibility.Never;
            });
        }
    }
}