namespace thinking1.Models
{
    public class UserAnswer
    {
        public string ScenarioId { get; set; } = string.Empty;
        public int StepNumber { get; set; }
        public int SelectedIndex { get; set; }
        public bool IsCorrect { get; set; }
    }
}
