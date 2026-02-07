using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

/// <summary>
/// Helper class for accessing custom field values (shadow properties) on entities.
/// Simplifies reading and writing custom field values through EF Core's Entry API.
/// </summary>
public class CustomFieldAccessor
{
    private readonly DbContext _dbContext;

    public CustomFieldAccessor(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets the value of a custom field from an entity.
    /// </summary>
    /// <typeparam name="T">The type of the custom field value</typeparam>
    /// <param name="entity">The entity containing the custom field</param>
    /// <param name="columnName">The column name of the custom field (e.g., "cfcustom_is_vip")</param>
    /// <returns>The value of the custom field, or default(T) if not set</returns>
    public T? GetValue<T>(object entity, string columnName)
    {
        var entry = _dbContext.Entry(entity);

        if (entry.State == EntityState.Detached)
            _dbContext.Attach(entity);

        // Use non-generic Property method to avoid type issues with nullables
        var value = entry.Property(columnName).CurrentValue;

        if (value == null)
            return default;

        return (T?)value;
    }

    /// <summary>
    /// Sets the value of a custom field on an entity.
    /// </summary>
    /// <typeparam name="T">The type of the custom field value</typeparam>
    /// <param name="entity">The entity containing the custom field</param>
    /// <param name="columnName">The column name of the custom field (e.g., "cfcustom_is_vip")</param>
    /// <param name="value">The value to set</param>
    public void SetValue<T>(object entity, string columnName, T? value)
    {
        var entry = _dbContext.Entry(entity);

        if (entry.State == EntityState.Detached)
            _dbContext.Attach(entity);

        // Use non-generic Property method to avoid type issues
        entry.Property(columnName).CurrentValue = value;
    }

    /// <summary>
    /// Gets all custom field values for an entity as a dictionary.
    /// Only returns fields with the "custom_" prefix.
    /// </summary>
    /// <param name="entity">The entity to get custom fields from</param>
    /// <returns>Dictionary of column name to value</returns>
    public Dictionary<string, object?> GetAllCustomFields(object entity)
    {
        var entry = _dbContext.Entry(entity);

        if (entry.State == EntityState.Detached)
            _dbContext.Attach(entity);

        var entityType = entry.Metadata;
        var result = new Dictionary<string, object?>();

        foreach (var property in entityType.GetProperties())
        {
            // Only include custom fields (shadow properties with "custom_" prefix)
            if (property.Name.StartsWith("cfcustom_") && property.IsShadowProperty())
            {
                var value = entry.Property(property.Name).CurrentValue;
                result[property.Name] = value;
            }
        }

        return result;
    }

    /// <summary>
    /// Sets multiple custom field values at once from a dictionary.
    /// </summary>
    /// <param name="entity">The entity to set custom fields on</param>
    /// <param name="customFields">Dictionary of column name to value</param>
    public void SetCustomFields(object entity, IReadOnlyDictionary<string, object?> customFields)
    {
        var entry = _dbContext.Entry(entity);

        if (entry.State == EntityState.Detached)
            _dbContext.Attach(entity);

        foreach (var (columnName, value) in customFields)
        {
            try
            {
                entry.Property(columnName).CurrentValue = value;
            }
            catch (InvalidOperationException)
            {
                // Property doesn't exist - skip it
                // This can happen if the custom field was deleted or doesn't exist for this entity type
                continue;
            }
        }
    }

    /// <summary>
    /// Checks if a custom field exists for an entity.
    /// </summary>
    /// <param name="entity">The entity to check</param>
    /// <param name="columnName">The column name to check for</param>
    /// <returns>True if the custom field exists, false otherwise</returns>
    public bool HasCustomField(object entity, string columnName)
    {
        var entry = _dbContext.Entry(entity);
        var entityType = entry.Metadata;

        return entityType.GetProperties()
            .Any(p => p.Name == columnName && p.IsShadowProperty());
    }
}