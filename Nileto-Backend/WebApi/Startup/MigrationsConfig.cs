using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Startup;

public static class MigrationsConfig
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        dbContext.Database.Migrate();
    }
}