namespace ArchSim.Models;

public class ScoreViewModel
{
    public Scenario Scenario { get; set; } = new();
    public int TotalScore { get; set; }
    public int MaxScore { get; set; }
    public ScoreBand Band { get; set; } = new();
    public List<StepReview> StepReviews { get; set; } = new();
}

public class StepReview
{
    public int StepNumber { get; set; }
    public string Question { get; set; } = "";
    public string Topic { get; set; } = "";
    public string Context { get; set; } = "";
    public string SelectedOptionText { get; set; } = "";
    public string SelectedOptionId { get; set; } = "";
    public int Score { get; set; }
    public int MaxScore { get; set; }
    public string Tag { get; set; } = "";
    public string TagColor { get; set; } = "";
}

public class WeakPointEntry
{
    public string Topic { get; set; } = "";
    public string Tag { get; set; } = "";
    public string TagColor { get; set; } = "";
}
