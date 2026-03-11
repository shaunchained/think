using ArchSim.Models;

namespace ArchSim.Services;

public class ScoreCalculator
{
    public ScoreViewModel Calculate(Scenario scenario, List<SessionAnswer> answers)
    {
        int total = answers.Sum(a => a.Score);
        int max = scenario.ScoreCard.MaxScore;

        var band = scenario.ScoreCard.Bands
            .FirstOrDefault(b => total >= b.Min && total <= b.Max)
            ?? scenario.ScoreCard.Bands.LastOrDefault()
            ?? new ScoreBand { Label = "Unknown", Color = "gray", Message = "" };

        var reviews = new List<StepReview>();
        foreach (var step in scenario.Steps)
        {
            var answer = answers.FirstOrDefault(a => a.StepNumber == step.StepNumber);
            var selectedOption = answer != null
                ? step.Options.FirstOrDefault(o => o.Id == answer.OptionId)
                : null;

            int stepMax = step.Options.Count > 0 ? step.Options.Max(o => o.Score) : 0;

            reviews.Add(new StepReview
            {
                StepNumber = step.StepNumber,
                Question = step.InterviewerSays.Length > 80
                    ? step.InterviewerSays[..80] + "..."
                    : step.InterviewerSays,
                SelectedOptionText = selectedOption?.Text ?? "(no answer)",
                SelectedOptionId = answer?.OptionId ?? "",
                Score = answer?.Score ?? 0,
                MaxScore = stepMax,
                Tag = selectedOption?.Tag ?? "Skipped",
                TagColor = selectedOption?.TagColor ?? "gray"
            });
        }

        return new ScoreViewModel
        {
            Scenario = scenario,
            TotalScore = total,
            MaxScore = max,
            Band = band,
            StepReviews = reviews
        };
    }
}
