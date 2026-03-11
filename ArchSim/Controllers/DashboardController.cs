using ArchSim.Data;
using ArchSim.Models;
using ArchSim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ArchSim.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly AppDbContext _db;
    private readonly ScenarioLoader _loader;

    public DashboardController(AppDbContext db, ScenarioLoader loader)
    {
        _db = db;
        _loader = loader;
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
                .FirstOrDefault()
        };

        return View(vm);
    }
}
