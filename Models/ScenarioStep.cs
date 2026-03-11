namespace thinking1.Models
{
    public class ScenarioStep
    {
        public int StepNumber { get; set; }
        public string Situation { get; set; } = string.Empty;
        public List<string> Logs { get; set; } = new();
        public string Question { get; set; } = string.Empty;
        public List<ScenarioOption> Options { get; set; } = new();
        public int CorrectIndex { get; set; }
        public string Explanation { get; set; } = string.Empty;
        public string SeniorSays { get; set; } = string.Empty;
    }
}
