namespace App.Domain.Entities;

public class Sprint
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Relationships
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public ICollection<TaskBase> Tasks { get; set; } = new List<TaskBase>();
}

