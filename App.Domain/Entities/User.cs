namespace App.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    // Relationships
    public ICollection<TaskBase> CreatedTasks { get; set; } = new List<TaskBase>();
    public ICollection<TaskBase> AssignedTasks { get; set; } = new List<TaskBase>();
}

