namespace ArchSim.Models;

public class UserScoreRecord
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public string ScenarioId { get; set; } = "";
    public string ScenarioTitle { get; set; } = "";
    public string Category { get; set; } = "";
    public string Difficulty { get; set; } = "";
    public int TotalScore { get; set; }
    public int MaxScore { get; set; }
    public string BandLabel { get; set; } = "";
    public string BandColor { get; set; } = "";
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    public string WeakPointsJson { get; set; } = "[]";
}
