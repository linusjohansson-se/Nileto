namespace Application.DTOs;

/// <summary>
/// Data transfer object for custom field information.
/// Used to expose custom field metadata to API consumers.
/// </summary>
public record CustomFieldDto(
    Guid Id,
    string EntityType,
    string FieldName,
    string ColumnName,
    string DataType,
    int? MaxLength,
    bool IsRequired,
    string? DefaultValue,
    string? DisplayName,
    string? Description,
    DateTime CreatedAt,
    string? CreatedBy
);

/// <summary>
/// Request DTO for creating a new custom field
/// </summary>
public record CreateCustomFieldRequest(
    string EntityType,
    string FieldName,
    string DataType,
    int? MaxLength = null,
    bool IsRequired = false,
    string? DefaultValue = null,
    string? DisplayName = null,
    string? Description = null
);