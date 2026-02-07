namespace Infrastructure.Database.Entities;

/// <summary>
/// Stores metadata about custom fields added to entities at runtime.
/// Each custom field corresponds to a real database column added via ALTER TABLE.
/// </summary>
public class CustomFieldDefinition
{
    public Guid Id { get; set; }

    /// <summary>
    /// The entity type this custom field belongs to (e.g., "Customer", "Order").
    /// Must match the EF entity type name.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// The user-friendly name of the custom field (e.g., "Is VIP Customer")
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// The actual column name in the database (e.g., "custom_is_vip").
    /// Using a standardized naming convention (custom_*) prevents SQL injection and naming conflicts.
    /// </summary>
    public string ColumnName { get; set; } = string.Empty;

    /// <summary>
    /// The data type: "string", "int", "long", "decimal", "bool", "date", "datetime", "guid"
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Maximum length for string types (VARCHAR constraint)
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Whether this field is required (NOT NULL constraint)
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Default value for the field (SQL expression, e.g., "false", "0", "CURRENT_TIMESTAMP")
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Display name for UI purposes (optional, falls back to FieldName)
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Description of what this field is for
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// When this custom field was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Who created this custom field (user ID or system identifier)
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Soft delete flag - we don't actually drop columns, just mark as deleted.
    /// Dropping columns in production is risky and can cause downtime.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// When this field was marked as deleted
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}