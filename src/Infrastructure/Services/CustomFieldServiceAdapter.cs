using Application.Abstractions;
using Application.DTOs;

namespace Infrastructure.Services;

/// <summary>
/// Adapter that implements the application interface and delegates to the infrastructure service.
/// Maps between domain DTOs and infrastructure entities.
/// </summary>
public class CustomFieldServiceAdapter : ICustomFieldService
{
    private readonly CustomFieldService _customFieldService;

    public CustomFieldServiceAdapter(CustomFieldService customFieldService)
    {
        _customFieldService = customFieldService;
    }

    public async Task<CustomFieldDto> CreateCustomFieldAsync(
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
        var entity = await _customFieldService.CreateCustomFieldAsync(
            entityType,
            fieldName,
            dataType,
            maxLength,
            isRequired,
            defaultValue,
            displayName,
            description,
            createdBy,
            cancellationToken);

        return MapToDto(entity);
    }

    public async Task DeleteCustomFieldAsync(
        Guid customFieldId,
        string? deletedBy = null,
        CancellationToken cancellationToken = default)
    {
        await _customFieldService.DeleteCustomFieldAsync(
            customFieldId,
            deletedBy,
            cancellationToken);
    }

    public async Task<List<CustomFieldDto>> GetCustomFieldsAsync(
        string entityType,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var entities = await _customFieldService.GetCustomFieldsAsync(
            entityType,
            includeDeleted,
            cancellationToken);

        return entities.Select(MapToDto).ToList();
    }

    public async Task<long> GetSchemaVersionAsync(CancellationToken cancellationToken = default)
    {
        return await _customFieldService.GetSchemaVersionAsync(cancellationToken);
    }

    private static CustomFieldDto MapToDto(Database.Entities.CustomFieldDefinition entity)
    {
        return new CustomFieldDto(
            entity.Id,
            entity.EntityType,
            entity.FieldName,
            entity.ColumnName,
            entity.DataType,
            entity.MaxLength,
            entity.IsRequired,
            entity.DefaultValue,
            entity.DisplayName,
            entity.Description,
            entity.CreatedAt,
            entity.CreatedBy
        );
    }
}