using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Infrastructure.Database;

/// <summary>
/// Custom model cache key factory that invalidates the EF Core model cache
/// when the custom field schema version changes.
/// This allows the application to pick up new custom fields without restarting.
/// </summary>
public class DynamicModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
    {
        // During design-time (migrations), use default cache key
        if (designTime)
            return (context.GetType(), designTime);

        if (context is ApplicationDbContext)
        {
            // Get the current schema version from the database
            var version = GetSchemaVersion(context);

            // Cache key includes the version - when version changes, cache is invalidated
            return (context.GetType(), version, designTime);
        }

        return (context.GetType(), designTime);
    }

    private static long GetSchemaVersion(DbContext context)
    {
        try
        {
            using var connection = context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
                connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT version
                FROM custom_field_schema_version
                WHERE id = 1";

            var result = command.ExecuteScalar();

            return result != null ? Convert.ToInt64(result) : 0;
        }
        catch
        {
            // If table doesn't exist yet (first run before migrations applied)
            // or any other error, return 0 as default version
            return 0;
        }
    }
}