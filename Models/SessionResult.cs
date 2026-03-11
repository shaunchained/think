namespace thinking1.Models
{
    public class SessionResult
    {
        public string ScenarioId { get; set; } = string.Empty;
        public string ScenarioTitle { get; set; } = string.Empty;
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int ScorePercent { get; set; }
        public string ScoreTitle { get; set; } = string.Empty;
        public string ScoreDescription { get; set; } = string.Empty;
        public List<AnswerReview> Answers { get; set; } = new();
    }
}
