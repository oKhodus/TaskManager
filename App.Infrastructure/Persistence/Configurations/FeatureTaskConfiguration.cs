using App.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public class FeatureTaskConfiguration : IEntityTypeConfiguration<FeatureTask>
{
    public void Configure(EntityTypeBuilder<FeatureTask> builder)
    {
        builder.Property(ft => ft.AcceptanceCriteria)
            .HasMaxLength(2000);
            
        builder.Property(ft => ft.Epic)
            .HasMaxLength(200);
            
        builder.Property(ft => ft.StoryPoints)
            .IsRequired(false);
    }
}


