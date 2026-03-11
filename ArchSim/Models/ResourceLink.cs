namespace ArchSim.Models;

public class ResourceLink
{
    public string Label { get; set; } = "";
    public string Url { get; set; } = "";
    public string Source { get; set; } = "";
}

public class ResourceGroup
{
    public string Category { get; set; } = "";
    public List<ResourceLink> Items { get; set; } = new();
}
