using App.Domain.Enums;

namespace App.Domain.Entities;

public abstract class TaskBase
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.ToDo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    
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
