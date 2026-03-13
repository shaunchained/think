namespace ArchSim.Models;

public class Scenario
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public string Difficulty { get; set; } = "";
    public int EstimatedMinutes { get; set; }
    public string Description { get; set; } = "";
    public string? Overview { get; set; }
    public int Sequence { get; set; }
    public List<string> Tags { get; set; } = new();
    public Interviewer Interviewer { get; set; } = new();
    public List<Step> Steps { get; set; } = new();
    public ScoreCard ScoreCard { get; set; } = new();
    public List<ResourceGroup> Resources { get; set; } = new();
}
