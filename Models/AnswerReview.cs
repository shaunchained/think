namespace thinking1.Models
{
    public class AnswerReview
    {
        public int StepNumber { get; set; }
        public string Question { get; set; } = string.Empty;
        public string SelectedOptionText { get; set; } = string.Empty;
        public string CorrectOptionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public string Explanation { get; set; } = string.Empty;
        public string SeniorSays { get; set; } = string.Empty;
    }
}
