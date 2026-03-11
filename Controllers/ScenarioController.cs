using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using thinking1.Models;
using thinking1.Services;

namespace thinking1.Controllers
{
    public class ScenarioController : Controller
    {
        private readonly ScenarioService _scenarioService;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ScenarioController(ScenarioService scenarioService)
        {
            _scenarioService = scenarioService;
        }

        // GET /Scenario/Start/{id}
        [HttpGet]
        public IActionResult Start(string id)
        {
            var scenario = _scenarioService.GetById(id);
            if (scenario == null) return NotFound();

            HttpContext.Session.SetString($"scenario_{id}_data", JsonSerializer.Serialize(scenario, _jsonOptions));
            HttpContext.Session.Remove($"scenario_{id}_answers");

            return RedirectToRoute("scenario_step", new { id, stepNumber = 1 });
        }

        // GET /Scenario/Step/{id}/{stepNumber}
        [HttpGet]
        public IActionResult Step(string id, int stepNumber)
        {
            var scenarioJson = HttpContext.Session.GetString($"scenario_{id}_data");
            if (scenarioJson == null)
                return RedirectToRoute("scenario_start", new { id });

            var scenario = JsonSerializer.Deserialize<FullScenario>(scenarioJson, _jsonOptions);
            if (scenario == null) return NotFound();

            if (stepNumber < 1 || stepNumber > scenario.Steps.Count)
                return RedirectToRoute("scenario_result", new { id });

            var step = scenario.Steps.FirstOrDefault(s => s.StepNumber == stepNumber);
            if (step == null)
                return RedirectToRoute("scenario_result", new { id });

            ViewBag.StepNumber = stepNumber;
            ViewBag.TotalSteps = scenario.Steps.Count;
            ViewBag.ScenarioTitle = scenario.Title;
            ViewBag.ScenarioType = scenario.Type;
            ViewBag.ScenarioId = id;

            // Pre-compute next URL server-side so the view never has to guess
            int nextStepNumber = stepNumber + 1;
            bool isLastStep = nextStepNumber > scenario.Steps.Count;
            ViewBag.NextUrl = isLastStep
                ? Url.RouteUrl("scenario_result", new { id })
                : Url.RouteUrl("scenario_step", new { id, stepNumber = nextStepNumber });
            ViewBag.IsLastStep = isLastStep;

            return View(step);
        }

        // POST /Scenario/Submit
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult Submit(string id, int stepNumber, int selectedIndex)
        {
            var scenarioJson = HttpContext.Session.GetString($"scenario_{id}_data");
            if (scenarioJson == null)
                return Json(new { error = "Session expired. Please start the scenario again." });

            var scenario = JsonSerializer.Deserialize<FullScenario>(scenarioJson, _jsonOptions);
            if (scenario == null)
                return Json(new { error = "Scenario not found." });

            var step = scenario.Steps.FirstOrDefault(s => s.StepNumber == stepNumber);
            if (step == null)
                return Json(new { error = "Step not found." });

            bool isCorrect = selectedIndex == step.CorrectIndex;

            // Load existing answers, upsert this step's answer
            var answersJson = HttpContext.Session.GetString($"scenario_{id}_answers");
            var answers = answersJson != null
                ? JsonSerializer.Deserialize<List<UserAnswer>>(answersJson, _jsonOptions) ?? new List<UserAnswer>()
                : new List<UserAnswer>();

            answers.RemoveAll(a => a.StepNumber == stepNumber);
            answers.Add(new UserAnswer
            {
                ScenarioId = id,
                StepNumber = stepNumber,
                SelectedIndex = selectedIndex,
                IsCorrect = isCorrect
            });

            HttpContext.Session.SetString($"scenario_{id}_answers", JsonSerializer.Serialize(answers, _jsonOptions));

            int nextStep = stepNumber + 1;
            bool isLastStep = nextStep > scenario.Steps.Count;
            string nextUrl = isLastStep
                ? Url.RouteUrl("scenario_result", new { id })!
                : Url.RouteUrl("scenario_step", new { id, stepNumber = nextStep })!;

            return Json(new
            {
                isCorrect,
                correctIndex = step.CorrectIndex,
                explanation = step.Explanation,
                seniorSays = step.SeniorSays,
                nextStepNumber = nextStep,
                isLastStep,
                nextUrl
            });
        }

        // GET /Scenario/Result/{id}
        [HttpGet]
        public IActionResult Result(string id)
        {
            var scenarioJson = HttpContext.Session.GetString($"scenario_{id}_data");
            if (scenarioJson == null)
                return RedirectToAction("Index", "Home");

            var scenario = JsonSerializer.Deserialize<FullScenario>(scenarioJson, _jsonOptions);
            if (scenario == null)
                return RedirectToAction("Index", "Home");

            var answersJson = HttpContext.Session.GetString($"scenario_{id}_answers");
            var answers = answersJson != null
                ? JsonSerializer.Deserialize<List<UserAnswer>>(answersJson, _jsonOptions) ?? new List<UserAnswer>()
                : new List<UserAnswer>();

            var reviewList = scenario.Steps.Select(step =>
            {
                var userAnswer = answers.FirstOrDefault(a => a.StepNumber == step.StepNumber);
                int selectedIndex = userAnswer?.SelectedIndex ?? -1;
                bool isCorrect = userAnswer?.IsCorrect ?? false;

                string selectedText = selectedIndex >= 0 && selectedIndex < step.Options.Count
                    ? $"{step.Options[selectedIndex].Key}. {step.Options[selectedIndex].Text}"
                    : "Not answered";

                string correctText = step.CorrectIndex >= 0 && step.CorrectIndex < step.Options.Count
                    ? $"{step.Options[step.CorrectIndex].Key}. {step.Options[step.CorrectIndex].Text}"
                    : string.Empty;

                return new AnswerReview
                {
                    StepNumber = step.StepNumber,
                    Question = step.Question,
                    SelectedOptionText = selectedText,
                    CorrectOptionText = correctText,
                    IsCorrect = isCorrect,
                    Explanation = step.Explanation,
                    SeniorSays = step.SeniorSays
                };
            }).ToList();

            int totalQuestions = scenario.Steps.Count;
            int correctAnswers = answers.Count(a => a.IsCorrect);
            int scorePercent = totalQuestions > 0
                ? (int)Math.Round((double)correctAnswers / totalQuestions * 100)
                : 0;

            string scoreTitle;
            string scoreDescription;

            if (scorePercent == 100)
            {
                scoreTitle = "Strategic Systems Thinking";
                scoreDescription = "This simulation reflects a senior, risk-aware lens under pressure — not a universal truth. Different teams, constraints, and risk appetites can justify different paths.The objective isn’t to “win” the scenario.It’s to sharpen your awareness of trade-offs.";
            }
            else if (scorePercent >= 75)
            {
                scoreTitle = "Senior Instincts";
                scoreDescription = "This simulation reflects a senior, risk-aware lens under pressure — not a universal truth. Different teams, constraints, and risk appetites can justify different paths.The objective isn’t to “win” the scenario.It’s to sharpen your awareness of trade-offs.";
            }
            else if (scorePercent >= 50)
            {
                scoreTitle = "Tactical Thinking";
                scoreDescription = "This simulation reflects a senior, risk-aware lens under pressure — not a universal truth. Different teams, constraints, and risk appetites can justify different paths.The objective isn’t to “win” the scenario.It’s to sharpen your awareness of trade-offs.";
            }
            else
            {
                scoreTitle = "Reactive Execution";
                scoreDescription = "This simulation reflects a senior, risk-aware lens under pressure — not a universal truth. Different teams, constraints, and risk appetites can justify different paths.The objective isn’t to “win” the scenario.It’s to sharpen your awareness of trade-offs.";
            }

            var result = new SessionResult
            {
                ScenarioId = id,
                ScenarioTitle = scenario.Title,
                TotalQuestions = totalQuestions,
                CorrectAnswers = correctAnswers,
                ScorePercent = scorePercent,
                ScoreTitle = scoreTitle,
                ScoreDescription = scoreDescription,
                Answers = reviewList
            };

            return View(result);
        }
    }
}
