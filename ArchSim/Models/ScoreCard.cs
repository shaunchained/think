namespace ArchSim.Models;

public class ScoreCard
{
    public int MaxScore { get; set; }
    public List<ScoreBand> Bands { get; set; } = new();
}
