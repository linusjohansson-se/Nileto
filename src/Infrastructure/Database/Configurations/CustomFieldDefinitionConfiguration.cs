using Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

public class CustomFieldDefinitionConfiguration : IEntityTypeConfiguration<CustomFieldDefinition>
{
    public void Configure(EntityTypeBuilder<CustomFieldDefinition> builder)
    {
        builder.ToTable("custom_field_definitions");

        builder.HasKey(f => f.Id);

        // Unique constraint: one custom field per entity type + column name (excluding deleted)
        builder.HasIndex(f => new { f.EntityType, f.ColumnName })
            .IsUnique()
            .HasFilter("is_deleted = false");

        // Index for querying by entity type
        builder.HasIndex(f => f.EntityType);

        // Property configurations
        builder.Property(f => f.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.FieldName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.ColumnName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.DataType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(f => f.DisplayName)
            .HasMaxLength(200);

        builder.Property(f => f.Description)
            .HasMaxLength(1000);

        builder.Property(f => f.CreatedBy)
            .HasMaxLength(100);

        builder.Property(f => f.CreatedAt)
            .IsRequired();

        builder.Property(f => f.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
    }
}