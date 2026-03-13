namespace ArchSim.Models;

public class Step
{
    public int StepNumber { get; set; }
    public string InterviewerSays { get; set; } = "";
    public string Topic { get; set; } = "";
    public string Context { get; set; } = "";
    public string Type { get; set; } = "single-choice";
    public List<AnswerOption> Options { get; set; } = new();
    public IdealAnswer IdealAnswer { get; set; } = new();
}
