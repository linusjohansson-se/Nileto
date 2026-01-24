using System.Data;
using Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<CustomFieldDefinition> CustomFieldDefinitions => Set<CustomFieldDefinition>();
    public DbSet<CustomFieldSchemaVersion> CustomFieldSchemaVersion => Set<CustomFieldSchemaVersion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(assembly: typeof(ApplicationDbContext).Assembly);

        modelBuilder.HasDefaultSchema(Schemas.Default);

        // Load custom fields as shadow properties
        LoadCustomFieldsIntoModel(modelBuilder);
    }

    /// <summary>
    /// Reads custom field definitions from the database and adds them as shadow properties
    /// to the EF Core model. This allows custom fields (added at runtime via ALTER TABLE)
    /// to be tracked and queried by EF Core.
    /// </summary>
    private void LoadCustomFieldsIntoModel(ModelBuilder modelBuilder)
    {
        var customFields = LoadCustomFieldDefinitions();

        foreach (var field in customFields)
        {
            try
            {
                // Get the entity builder for this type
                var entityBuilder = modelBuilder.Entity(field.EntityType);

                // Map to CLR type
                var clrType = MapToClrType(field.DataType);

                // Add shadow property
                var property = entityBuilder.Property(clrType, field.ColumnName);

                // Apply constraints
                if (field.IsRequired)
                    property.IsRequired();

                if (field.MaxLength.HasValue && field.DataType == "string")
                    property.HasMaxLength(field.MaxLength.Value);
            }
            catch (InvalidOperationException)
            {
                // Entity type not found in model - skip this custom field
                // This can happen if a custom field was added for an entity that no longer exists
                continue;
            }
        }
    }

    /// <summary>
    /// Reads custom field definitions from the database during model building.
    /// Uses raw ADO.NET to avoid circular dependency (can't use EF to build EF model).
    /// </summary>
    private List<CustomFieldDefinition> LoadCustomFieldDefinitions()
    {
        var fields = new List<CustomFieldDefinition>();

        try
        {
            using var connection = Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
                connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT entity_type, column_name, data_type, max_length, is_required
                FROM custom_field_definitions
                WHERE is_deleted = false
                ORDER BY created_at";

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                fields.Add(new CustomFieldDefinition
                {
                    EntityType = reader.GetString(0),
                    ColumnName = reader.GetString(1),
                    DataType = reader.GetString(2),
                    MaxLength = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    IsRequired = reader.GetBoolean(4)
                });
            }
        }
        catch
        {
            // If table doesn't exist yet (first run before migrations)
            // or any other error, just return empty list
        }

        return fields;
    }

    /// <summary>
    /// Maps custom field data types to CLR types for EF Core shadow properties.
    /// All types are nullable to support existing data and optional fields.
    /// </summary>
    private static Type MapToClrType(string dataType) => dataType switch
    {
        "string" => typeof(string),
        "int" => typeof(int?),
        "long" => typeof(long?),
        "decimal" => typeof(decimal?),
        "bool" => typeof(bool?),
        "date" => typeof(DateOnly?),
        "datetime" => typeof(DateTime?),
        "guid" => typeof(Guid?),
        _ => typeof(string) // Default to string for unknown types
    };
}