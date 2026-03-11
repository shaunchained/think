namespace ArchSim.Models;

public class AnswerOption
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    public int Score { get; set; }
    public string Tag { get; set; } = "";
    public string TagColor { get; set; } = "";
    public string InterviewerReaction { get; set; } = "";
}
