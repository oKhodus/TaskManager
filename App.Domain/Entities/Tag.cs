namespace App.Domain.Entities;

public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#808080"; // Default gray color
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // N-N relationship with Tasks
    public ICollection<TaskBase> Tasks { get; set; } = new List<TaskBase>();
}


