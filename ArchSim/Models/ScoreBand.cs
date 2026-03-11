namespace ArchSim.Models;

public class ScoreBand
{
    public int Min { get; set; }
    public int Max { get; set; }
    public string Label { get; set; } = "";
    public string Color { get; set; } = "";
    public string Message { get; set; } = "";
}
