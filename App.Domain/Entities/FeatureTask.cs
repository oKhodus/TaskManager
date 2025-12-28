namespace App.Domain.Entities;

public class FeatureTask : TaskBase
{
    public override string TaskTypeName => "Feature";

    public string AcceptanceCriteria { get; set; } = string.Empty;
    public int? StoryPoints { get; set; }
    public string? Epic { get; set; }
}

