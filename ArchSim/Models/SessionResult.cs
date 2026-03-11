namespace ArchSim.Models;

public class SessionResult
{
    public string ScenarioId { get; set; } = "";
    public List<SessionAnswer> Answers { get; set; } = new();
}
