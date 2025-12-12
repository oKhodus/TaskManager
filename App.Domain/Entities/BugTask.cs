namespace App.Domain.Entities;

public class BugTask : TaskBase
{
    public string? StepsToReproduce { get; set; } = string.Empty;
    public string? ExpectedBehavior { get; set; }
    public string? ActualBehavior { get; set; }
    public string? Environment { get; set; }
    public string? Severity { get; set; }
}


