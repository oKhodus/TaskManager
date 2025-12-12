using App.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public class SprintConfiguration : IEntityTypeConfiguration<Sprint>
{
    public void Configure(EntityTypeBuilder<Sprint> builder)
    {
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(s => s.Description)
            .HasMaxLength(1000);
            
        builder.Property(s => s.StartDate)
            .IsRequired();
            
        builder.Property(s => s.EndDate)
            .IsRequired();
            
        builder.Property(s => s.CreatedAt)
            .IsRequired();
            
        // 1-N: Project -> Sprints
        builder.HasOne(s => s.Project)
            .WithMany(p => p.Sprints)
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // 1-N: Sprint -> Tasks
        builder.HasMany(s => s.Tasks)
            .WithOne(t => t.Sprint)
            .HasForeignKey(t => t.SprintId)
            .OnDelete(DeleteBehavior.SetNull);
            
        // Indexes
        builder.HasIndex(s => s.ProjectId);
        builder.HasIndex(s => s.IsActive);
        builder.HasIndex(s => new { s.StartDate, s.EndDate });
    }
}


