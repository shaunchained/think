namespace ArchSim.Models;

public class ScenarioSummary
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public string Difficulty { get; set; } = "";
    public int EstimatedMinutes { get; set; }
    public string Description { get; set; } = "";
    public List<string> Tags { get; set; } = new();
    public int TotalSteps { get; set; }
    public int Sequence { get; set; }
}
