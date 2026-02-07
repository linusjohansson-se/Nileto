using Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

public class CustomFieldSchemaVersionConfiguration : IEntityTypeConfiguration<CustomFieldSchemaVersion>
{
    public void Configure(EntityTypeBuilder<CustomFieldSchemaVersion> builder)
    {
        builder.ToTable("custom_field_schema_version");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .ValueGeneratedNever(); // We manually set this to 1

        builder.Property(v => v.Version)
            .IsRequired();

        builder.Property(v => v.LastModified)
            .IsRequired();

        builder.Property(v => v.LastModifiedBy)
            .HasMaxLength(100);

        // Initialize with a single row (version 0)
        builder.HasData(new CustomFieldSchemaVersion
        {
            Id = 1,
            Version = 0,
            LastModified = DateTime.UtcNow
        });
    }
}