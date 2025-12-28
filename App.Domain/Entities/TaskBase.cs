using App.Domain.Enums;

namespace App.Domain.Entities;

public abstract class TaskBase
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Type of the task (e.g., "Bug", "Feature").
    /// Must be implemented by derived classes for UI display and extensibility.
    /// </summary>
    public abstract string TaskTypeName { get; }
    
    // Relationships
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public Guid? SprintId { get; set; }
    public Sprint? Sprint { get; set; }
    
    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;
    
    public Guid? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }
    
    // N-N relationship with Tags
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
