using App.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public class BugTaskConfiguration : IEntityTypeConfiguration<BugTask>
{
    public void Configure(EntityTypeBuilder<BugTask> builder)
    {
        builder.Property(bt => bt.StepsToReproduce)
            .HasMaxLength(2000);
            
        builder.Property(bt => bt.ExpectedBehavior)
            .HasMaxLength(1000);
            
        builder.Property(bt => bt.ActualBehavior)
            .HasMaxLength(1000);
            
        builder.Property(bt => bt.Environment)
            .HasMaxLength(200);
            
        builder.Property(bt => bt.Severity)
            .HasMaxLength(50);
    }
}

