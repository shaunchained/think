namespace ArchSim.Models;

public class IdealAnswerSection
{
    public string Type { get; set; } = "";
    public string Heading { get; set; } = "";
    public string? Content { get; set; }
    public List<string>? Items { get; set; }
    public List<string>? Pros { get; set; }
    public List<string>? Cons { get; set; }
}
