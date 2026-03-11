namespace ArchSim.Models;

public class PostRead
{
    public string Title { get; set; } = "";
    public string Intro { get; set; } = "";
    public List<IdealAnswerSection> Sections { get; set; } = new();
}
