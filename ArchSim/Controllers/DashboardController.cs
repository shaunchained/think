using ArchSim.Data;
using ArchSim.Models;
using ArchSim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace ArchSim.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly AppDbContext _db;
    private readonly ScenarioLoader _loader;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    public DashboardController(AppDbContext db, ScenarioLoader loader, IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _db = db;
        _loader = loader;
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    public IActionResult Index()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = _db.Users
            .Include(u => u.ScoreRecords)
            .FirstOrDefault(u => u.Id == userId);

        if (user == null) return NotFound();

        var allScenarios = _loader.GetAll();
        var completedScenarioIds = user.ScoreRecords.Select(r => r.ScenarioId).Distinct().ToHashSet();

        // Aggregate weak points across all score records, deduped by context
        var weakAreas = user.ScoreRecords
            .SelectMany(r =>
            {
                try { return JsonSerializer.Deserialize<List<WeakPointEntry>>(r.WeakPointsJson) ?? new(); }
                catch { return new List<WeakPointEntry>(); }
            })
            .Where(p => !string.IsNullOrWhiteSpace(p.Topic))
            .GroupBy(p => p.Topic)
            .Select(g => g.First())
            .ToList();

        // Recommend scenarios not yet mastered, ranked by tag overlap with weak scenarios
        var weakScenarioIds = user.ScoreRecords
            .Where(r => r.BandColor != "green")
            .Select(r => r.ScenarioId)
            .ToHashSet();

        var weakTags = allScenarios
            .Where(s => weakScenarioIds.Contains(s.Id))
            .SelectMany(s => s.Tags)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var recommendedScenarios = allScenarios
            .Where(s => !user.ScoreRecords.Any(r => r.ScenarioId == s.Id && r.BandColor == "green"))
            .OrderByDescending(s => weakTags.Any() ? s.Tags.Count(t => weakTags.Contains(t)) : 0)
            .ThenBy(s => s.Sequence)
            .Take(3)
            .ToList();

        var vm = new DashboardViewModel
        {
            User = user,
            ScoreRecords = user.ScoreRecords.OrderByDescending(r => r.CompletedAt).ToList(),
            TotalScenarios = allScenarios.Count,
            CompletedScenarios = completedScenarioIds.Count,
            AveragePercentage = user.ScoreRecords.Any()
                ? (int)Math.Round(user.ScoreRecords.Average(r => r.MaxScore > 0 ? (r.TotalScore * 100.0 / r.MaxScore) : 0))
                : 0,
            BestRecord = user.ScoreRecords
                .OrderByDescending(r => r.MaxScore > 0 ? (r.TotalScore * 100.0 / r.MaxScore) : 0)
                .FirstOrDefault(),
            WeakAreas = weakAreas,
            RecommendedScenarios = recommendedScenarios
        };

        return View(vm);
    }

    public IActionResult StudyPlan()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = _db.Users
            .Include(u => u.ScoreRecords)
            .FirstOrDefault(u => u.Id == userId);

        if (user == null) return NotFound();

        var weakAreas = user.ScoreRecords
            .SelectMany(r =>
            {
                try { return JsonSerializer.Deserialize<List<WeakPointEntry>>(r.WeakPointsJson) ?? new(); }
                catch { return new List<WeakPointEntry>(); }
            })
            .Where(p => !string.IsNullOrWhiteSpace(p.Topic))
            .GroupBy(p => p.Topic)
            .Select(g => g.First())
            .ToList();

        if (!weakAreas.Any()) return RedirectToAction("Index");

        return View(weakAreas);
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> GenerateTopicContent([FromForm] string topic, [FromForm] string tag)
    {
        var apiKey = _config["Anthropic:ApiKey"] ?? "";
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_ANTHROPIC_API_KEY_HERE")
            return Json(new { error = "Anthropic API key is not configured." });

        var prompt = $$"""
            A software engineering candidate scored poorly on: "{{topic}}" (performance: {{tag}}).

            Your job is to teach this concept so clearly that even someone who completely misunderstood it will walk away confident.

            Rules:
            - "what": Explain the concept in plain English as if talking to a smart developer who has never encountered it. Use an analogy if it helps. 4-6 sentences. No jargon without explanation.
            - "why": Explain specifically why this concept comes up in system design interviews and what goes wrong in production when it is misunderstood. 2-3 sentences.
            - "keyPoints": Exactly 4 items. Each must be a concrete, memorable rule — not vague advice. Write them as things the candidate should be able to recite and apply immediately.
            - "example": Tell a specific real-world story. Name a realistic company/system (e.g. "an e-commerce platform like Amazon", "a ride-sharing app like Uber"). Describe the exact problem that occurs without this concept, then how applying this concept solves it. 4-6 sentences. Make it feel real, not textbook.

            Return ONLY a valid JSON object in this exact format — no markdown, no extra text, no explanation outside the JSON:
            {
              "what": "...",
              "why": "...",
              "keyPoints": ["...", "...", "...", "..."],
              "example": "..."
            }
            """;

        var requestBody = JsonSerializer.Serialize(new
        {
            model = "claude-haiku-4-5-20251001",
            max_tokens = 1200,
            messages = new[] { new { role = "user", content = prompt } }
        });

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PostAsync(
            "https://api.anthropic.com/v1/messages",
            new StringContent(requestBody, Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
            return Json(new { error = "Failed to generate content. Please try again." });

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);
        var text = doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? "{}";

        // Strip markdown code fences if Claude wraps the JSON
        var cleaned = text.Trim();
        if (cleaned.StartsWith("```"))
        {
            var firstNewline = cleaned.IndexOf('\n');
            if (firstNewline >= 0) cleaned = cleaned[(firstNewline + 1)..];
            if (cleaned.EndsWith("```")) cleaned = cleaned[..^3].TrimEnd();
        }

        try
        {
            using var contentDoc = JsonDocument.Parse(cleaned);
            return Content(cleaned, "application/json");
        }
        catch
        {
            return Json(new { error = "Failed to parse generated content." });
        }
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> GenerateSummary()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = _db.Users
            .Include(u => u.ScoreRecords)
            .FirstOrDefault(u => u.Id == userId);

        if (user == null) return NotFound();

        var weakAreas = user.ScoreRecords
            .SelectMany(r =>
            {
                try { return JsonSerializer.Deserialize<List<WeakPointEntry>>(r.WeakPointsJson) ?? new(); }
                catch { return new List<WeakPointEntry>(); }
            })
            .Where(p => !string.IsNullOrWhiteSpace(p.Topic))
            .GroupBy(p => p.Topic)
            .Select(g => g.First())
            .ToList();

        if (!weakAreas.Any())
            return Json(new { summary = "No weak areas identified yet. Complete some scenarios to see your improvement areas." });

        var apiKey = _config["Anthropic:ApiKey"] ?? "";
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_ANTHROPIC_API_KEY_HERE")
            return Json(new { error = "Anthropic API key is not configured." });

        var topicList = string.Join("\n", weakAreas.Select(w => $"- {w.Topic} ({w.Tag})"));
        var prompt = $"""
            A candidate attempted architecture interview scenarios. Here are their weak areas with performance labels:

            {topicList}

            Write a very short, direct summary (3-5 sentences max, plain text, no markdown, no bullet points).
            Only tell them WHAT they are weak at — do not explain the concepts or give study advice.
            Be blunt and clear. Start with "Your weak areas are:" and list them naturally in one or two sentences.
            End with one short sentence on the overall pattern you see across these weaknesses.
            """;

        var requestBody = JsonSerializer.Serialize(new
        {
            model = "claude-haiku-4-5-20251001",
            max_tokens = 200,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        });

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PostAsync(
            "https://api.anthropic.com/v1/messages",
            new StringContent(requestBody, Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
            return Json(new { error = "Failed to generate summary. Please try again." });

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);
        var text = doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? "";

        return Json(new { summary = text });
    }
}
