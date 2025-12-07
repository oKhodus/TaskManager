using App.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(t => t.Color)
            .IsRequired()
            .HasMaxLength(7) // Hex color format: #RRGGBB
            .HasDefaultValue("#808080");
            
        builder.Property(t => t.CreatedAt)
            .IsRequired();
            
        // N-N: Tags <-> Tasks (configured in TaskBaseConfiguration)
        // This is handled via the join table "TaskTags"
        
        // Indexes
        builder.HasIndex(t => t.Name)
            .IsUnique();
    }
}

