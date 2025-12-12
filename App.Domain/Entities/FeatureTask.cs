namespace App.Domain.Entities;

public class FeatureTask : TaskBase
{
    public string? AcceptanceCriteria { get; set; } = string.Empty;
    public int? StoryPoints { get; set; }
    public string? Epic { get; set; }
}


