using System.Text.RegularExpressions;
using Infrastructure.Database;
using Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

/// <summary>
/// Service for managing custom fields at runtime.
/// Handles ALTER TABLE operations and schema version management.
/// </summary>
public class CustomFieldService
{
    private readonly ApplicationDbContext _dbContext;

    public CustomFieldService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Creates a new custom field by:
    /// 1. Validating the input
    /// 2. Executing ALTER TABLE to add the column
    /// 3. Saving metadata
    /// 4. Incrementing schema version (triggers model cache invalidation)
    /// </summary>
    public async Task<CustomFieldDefinition> CreateCustomFieldAsync(
        string entityType,
        string fieldName,
        string dataType,
        int? maxLength = null,
        bool isRequired = false,
        string? defaultValue = null,
        string? displayName = null,
        string? description = null,
        string? createdBy = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Validate input
        ValidateFieldName(fieldName);
        ValidateDataType(dataType);

        if (dataType == "string" && maxLength.HasValue && maxLength.Value <= 0)
            throw new ArgumentException("MaxLength must be positive for string types", nameof(maxLength));

        // 2. Generate safe column name
        var columnName = GenerateColumnName(fieldName);

        // 3. Check if field already exists
        var exists = await _dbContext.CustomFieldDefinitions
            .AnyAsync(f => f.EntityType == entityType &&
                          f.ColumnName == columnName &&
                          !f.IsDeleted,
                cancellationToken);

        if (exists)
            throw new InvalidOperationException(
                $"Custom field '{fieldName}' already exists for {entityType}");

        // 4. Get table name for this entity type
        var tableName = GetTableName(entityType);

        // 5. Build SQL type definition
        var sqlType = BuildSqlType(dataType, maxLength, isRequired, defaultValue);

        // 6. Execute ALTER TABLE
        var alterSql = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {sqlType}";
        await _dbContext.Database.ExecuteSqlRawAsync(alterSql, cancellationToken);

        // 7. Create metadata record
        var fieldDef = new CustomFieldDefinition
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            FieldName = fieldName,
            ColumnName = columnName,
            DataType = dataType,
            MaxLength = maxLength,
            IsRequired = isRequired,
            DefaultValue = defaultValue,
            DisplayName = displayName ?? fieldName,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            IsDeleted = false
        };

        _dbContext.CustomFieldDefinitions.Add(fieldDef);

        // 8. Increment schema version (CRITICAL - this invalidates EF model cache)
        await IncrementSchemaVersionAsync(createdBy, cancellationToken);

        // 9. Save all changes in a single transaction
        await _dbContext.SaveChangesAsync(cancellationToken);

        return fieldDef;
    }

    /// <summary>
    /// Soft-deletes a custom field. The column remains in the database but is ignored by EF.
    /// </summary>
    public async Task DeleteCustomFieldAsync(
        Guid customFieldId,
        string? deletedBy = null,
        CancellationToken cancellationToken = default)
    {
        var field = await _dbContext.CustomFieldDefinitions
            .FirstOrDefaultAsync(f => f.Id == customFieldId && !f.IsDeleted, cancellationToken);

        if (field == null)
            throw new InvalidOperationException($"Custom field with ID {customFieldId} not found");

        // Mark as deleted (we don't DROP COLUMN - too risky in production)
        field.IsDeleted = true;
        field.DeletedAt = DateTime.UtcNow;

        // Increment schema version to trigger model rebuild
        await IncrementSchemaVersionAsync(deletedBy, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all custom fields for a specific entity type
    /// </summary>
    public async Task<List<CustomFieldDefinition>> GetCustomFieldsAsync(
        string entityType,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.CustomFieldDefinitions
            .Where(f => f.EntityType == entityType);

        if (!includeDeleted)
            query = query.Where(f => !f.IsDeleted);

        return await query
            .OrderBy(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the current schema version
    /// </summary>
    public async Task<long> GetSchemaVersionAsync(CancellationToken cancellationToken = default)
    {
        var version = await _dbContext.CustomFieldSchemaVersion
            .Where(v => v.Id == 1)
            .Select(v => v.Version)
            .FirstOrDefaultAsync(cancellationToken);

        return version;
    }

    /// <summary>
    /// Increments the schema version atomically.
    /// This causes all application instances to invalidate their EF model cache.
    /// </summary>
    private async Task IncrementSchemaVersionAsync(
        string? modifiedBy,
        CancellationToken cancellationToken)
    {
        // Use raw SQL for atomic increment
        await _dbContext.Database.ExecuteSqlRawAsync(@"
            UPDATE custom_field_schema_version
            SET version = version + 1,
                last_modified = @p0,
                last_modified_by = @p1
            WHERE id = 1",
            [DateTime.UtcNow, modifiedBy ?? "system"],
            cancellationToken);
    }

    /// <summary>
    /// Generates a safe column name from a user-provided field name.
    /// Format: custom_{sanitized_lowercase_name}
    /// </summary>
    private static string GenerateColumnName(string fieldName)
    {
        // Convert to lowercase and replace spaces with underscores
        var sanitized = fieldName
            .ToLowerInvariant()
            .Replace(" ", "_")
            .Trim();

        // Remove any non-alphanumeric characters except underscore
        sanitized = Regex.Replace(sanitized, @"[^a-z0-9_]", "");

        // Ensure it starts with a letter (prepend 'f' if it doesn't)
        if (!char.IsLetter(sanitized[0]))
            sanitized = "f_" + sanitized;

        return $"cfcustom_{sanitized}";
    }

    /// <summary>
    /// Builds the SQL type definition for the column
    /// </summary>
    private static string BuildSqlType(
        string dataType,
        int? maxLength,
        bool isRequired,
        string? defaultValue)
    {
        var sqlType = dataType switch
        {
            "string" => maxLength.HasValue ? $"VARCHAR({maxLength.Value})" : "TEXT",
            "int" => "INTEGER",
            "long" => "BIGINT",
            "decimal" => "DECIMAL(18, 2)",
            "bool" => "BOOLEAN",
            "date" => "DATE",
            "datetime" => "TIMESTAMP",
            "guid" => "UUID",
            _ => throw new ArgumentException($"Unknown data type: {dataType}")
        };

        // Add NULL/NOT NULL constraint
        sqlType += isRequired ? " NOT NULL" : " NULL";

        // Add default value if specified
        if (defaultValue != null)
            sqlType += $" DEFAULT {defaultValue}";

        return sqlType;
    }

    /// <summary>
    /// Validates field name format
    /// </summary>
    private static void ValidateFieldName(string fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            throw new ArgumentException("Field name cannot be empty", nameof(fieldName));

        if (fieldName.Length > 200)
            throw new ArgumentException("Field name is too long (max 200 characters)", nameof(fieldName));

        if (!Regex.IsMatch(fieldName, @"^[a-zA-Z][a-zA-Z0-9_ ]*$"))
            throw new ArgumentException(
                "Field name must start with a letter and contain only letters, numbers, spaces, and underscores",
                nameof(fieldName));
    }

    /// <summary>
    /// Validates data type
    /// </summary>
    private static void ValidateDataType(string dataType)
    {
        var validTypes = new[] { "string", "int", "long", "decimal", "bool", "date", "datetime", "guid" };

        if (!validTypes.Contains(dataType))
            throw new ArgumentException(
                $"Invalid data type. Must be one of: {string.Join(", ", validTypes)}",
                nameof(dataType));
    }

    /// <summary>
    /// Gets the database table name for an entity type using EF metadata
    /// </summary>
    private string GetTableName(string entityType)
    {
        var entityTypeObj = _dbContext.Model.FindEntityType(entityType)
            ?? throw new InvalidOperationException(
                $"Entity type '{entityType}' not found in EF model. " +
                $"Ensure the entity is registered in your DbContext.");

        var tableName = entityTypeObj.GetTableName()
            ?? throw new InvalidOperationException(
                $"Entity type '{entityType}' is not mapped to a database table.");

        var schema = entityTypeObj.GetSchema();

        return schema != null ? $"{schema}.{tableName}" : tableName;
    }
}