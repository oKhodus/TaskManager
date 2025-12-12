using App.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects { get; set; }
    public DbSet<TaskBase> Tasks { get; set; }
    public DbSet<BugTask> BugTasks { get; set; }
    public DbSet<FeatureTask> FeatureTasks { get; set; }
    public DbSet<Sprint> Sprints { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Tag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}


