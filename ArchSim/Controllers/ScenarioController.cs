using ArchSim.Models;
using ArchSim.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ArchSim.Controllers;

public class ScenarioController : Controller
{
    private readonly ScenarioLoader _loader;
    private readonly ScoreCalculator _calculator;

    public ScenarioController(ScenarioLoader loader, ScoreCalculator calculator)
    {
        _loader = loader;
        _calculator = calculator;
    }

    // GET /scenario/{id}
    public IActionResult Start(string id)
    {
        var scenario = _loader.GetById(id);
        if (scenario == null) return NotFound();

        // Initialize empty session state
        var result = new SessionResult { ScenarioId = id, Answers = new List<SessionAnswer>() };
        HttpContext.Session.SetString($"answers_{id}", JsonSerializer.Serialize(result.Answers));

        return RedirectToAction("Step", new { id, n = 1 });
    }

    // GET /scenario/{id}/step/{n}
    public IActionResult Step(string id, int n)
    {
        var scenario = _loader.GetById(id);
        if (scenario == null) return NotFound();

        var step = scenario.Steps.FirstOrDefault(s => s.StepNumber == n);
        if (step == null) return NotFound();

        ViewBag.Scenario = scenario;
        ViewBag.TotalSteps = scenario.Steps.Count;
        return View("Player", step);
    }

    // POST /scenario/{id}/answer
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public IActionResult Answer(string id, [FromForm] int stepNumber, [FromForm] string selectedOptionId)
    {
        var scenario = _loader.GetById(id);
        if (scenario == null) return NotFound();

        var step = scenario.Steps.FirstOrDefault(s => s.StepNumber == stepNumber);
        if (step == null) return BadRequest();

        var option = step.Options.FirstOrDefault(o => o.Id == selectedOptionId);
        if (option == null) return BadRequest();

        // Append to session
        var answersJson = HttpContext.Session.GetString($"answers_{id}") ?? "[]";
        var answers = JsonSerializer.Deserialize<List<SessionAnswer>>(answersJson) ?? new();

        // Replace if already answered this step
        answers.RemoveAll(a => a.StepNumber == stepNumber);
        answers.Add(new SessionAnswer
        {
            StepNumber = stepNumber,
            OptionId = selectedOptionId,
            Score = option.Score
        });

        HttpContext.Session.SetString($"answers_{id}", JsonSerializer.Serialize(answers));

        bool isLastStep = stepNumber >= scenario.Steps.Count;
        var bestOption = step.Options.OrderByDescending(o => o.Score).First();

        return Json(new
        {
            score = option.Score,
            tag = option.Tag,
            tagColor = option.TagColor,
            interviewerReaction = option.InterviewerReaction,
            idealAnswer = step.IdealAnswer,
            isLastStep,
            nextStepNumber = isLastStep ? 0 : stepNumber + 1,
            bestOptionId = bestOption.Id
        });
    }

    // GET /scenario/{id}/score
    public IActionResult Score(string id)
    {
        var scenario = _loader.GetById(id);
        if (scenario == null) return NotFound();

        var answersJson = HttpContext.Session.GetString($"answers_{id}") ?? "[]";
        var answers = JsonSerializer.Deserialize<List<SessionAnswer>>(answersJson) ?? new();

        var vm = _calculator.Calculate(scenario, answers);
        return View(vm);
    }
}
