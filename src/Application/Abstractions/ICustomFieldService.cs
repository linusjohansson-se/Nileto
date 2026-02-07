using Application.DTOs;

namespace Application.Abstractions;

/// <summary>
/// Service interface for managing custom fields at runtime.
/// Implementations handle ALTER TABLE operations and schema versioning.
/// </summary>
public interface ICustomFieldService
{
    /// <summary>
    /// Creates a new custom field for the specified entity type.
    /// Adds a real database column and updates EF model.
    /// </summary>
    Task<CustomFieldDto> CreateCustomFieldAsync(
        string entityType,
        string fieldName,
        string dataType,
        int? maxLength = null,
        bool isRequired = false,
        string? defaultValue = null,
        string? displayName = null,
        string? description = null,
        string? createdBy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes a custom field. The column remains in the database but is ignored.
    /// </summary>
    Task DeleteCustomFieldAsync(
        Guid customFieldId,
        string? deletedBy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all custom fields for a specific entity type.
    /// </summary>
    Task<List<CustomFieldDto>> GetCustomFieldsAsync(
        string entityType,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current schema version.
    /// </summary>
    Task<long> GetSchemaVersionAsync(CancellationToken cancellationToken = default);
}