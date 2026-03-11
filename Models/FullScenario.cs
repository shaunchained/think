namespace thinking1.Models
{
    public class FullScenario
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public int EstimatedMinutes { get; set; }
        public bool IsAvailable { get; set; }
        public int Sequence { get; set; }
        public List<ScenarioStep> Steps { get; set; } = new();
    }
}
