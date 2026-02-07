namespace Infrastructure.Database.Entities;

/// <summary>
/// Tracks the current schema version for custom fields.
/// This table contains exactly one row that gets updated when schema changes.
/// Used by EF Core's model cache to determine when to rebuild the model.
/// </summary>
public class CustomFieldSchemaVersion
{
    /// <summary>
    /// Always 1 - this is a single-row table
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Increments with each schema change (custom field add/remove).
    /// All application instances check this version to know when to rebuild their EF model.
    /// </summary>
    public long Version { get; set; }

    /// <summary>
    /// When the schema was last modified
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Who made the last schema change (optional tracking)
    /// </summary>
    public string? LastModifiedBy { get; set; }
}