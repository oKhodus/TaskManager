using App.Domain.Entities;
using App.Domain.Enums;
using TaskStatus = App.Domain.Enums.TaskStatus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public class TaskBaseConfiguration : IEntityTypeConfiguration<TaskBase>
{
    public void Configure(EntityTypeBuilder<TaskBase> builder)
    {
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(t => t.Description)
            .HasMaxLength(2000);
            
        builder.Property(t => t.Status)
            .HasConversion<int>()
            .IsRequired();
            
        builder.Property(t => t.Priority)
            .HasConversion<int>()
            .IsRequired();
            
        builder.Property(t => t.CreatedAt)
            .IsRequired();
            
        // TPH (Table Per Hierarchy) strategy for inheritance
        builder.HasDiscriminator<TaskType>("TaskType")
            .HasValue<BugTask>(TaskType.Bug)
            .HasValue<FeatureTask>(TaskType.Feature);
            
        // 1-N: Project -> Tasks
        builder.HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // 1-N: Sprint -> Tasks (optional)
        builder.HasOne(t => t.Sprint)
            .WithMany(s => s.Tasks)
            .HasForeignKey(t => t.SprintId)
            .OnDelete(DeleteBehavior.SetNull);
            
        // 1-N: User -> Created Tasks
        builder.HasOne(t => t.CreatedBy)
            .WithMany(u => u.CreatedTasks)
            .HasForeignKey(t => t.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
            
        // 1-N: User -> Assigned Tasks (optional)
        builder.HasOne(t => t.AssignedTo)
            .WithMany(u => u.AssignedTasks)
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);
            
        // N-N: Tasks <-> Tags
        builder.HasMany(t => t.Tags)
            .WithMany(t => t.Tasks)
            .UsingEntity<Dictionary<string, object>>(
                "TaskTag",
                j => j.HasOne<Tag>().WithMany().HasForeignKey("TagId"),
                j => j.HasOne<TaskBase>().WithMany().HasForeignKey("TaskId"),
                j =>
                {
                    j.HasKey("TaskId", "TagId");
                    j.ToTable("TaskTags");
                });
                
        // Indexes
        builder.HasIndex(t => t.ProjectId);
        builder.HasIndex(t => t.SprintId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.Priority);
        builder.HasIndex(t => t.CreatedById);
        builder.HasIndex(t => t.AssignedToId);
    }
}

