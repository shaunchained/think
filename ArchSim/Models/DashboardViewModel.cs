namespace ArchSim.Models;

public class DashboardViewModel
{
    public ApplicationUser User { get; set; } = null!;
    public List<UserScoreRecord> ScoreRecords { get; set; } = new();
    public int TotalScenarios { get; set; }
    public int CompletedScenarios { get; set; }
    public int AveragePercentage { get; set; }
    public UserScoreRecord? BestRecord { get; set; }
}
