namespace ArchSim.Models;

public class IdealAnswer
{
    public string Summary { get; set; } = "";
    public List<IdealAnswerSection> Sections { get; set; } = new();
    public string OneLineSummary { get; set; } = "";
}
