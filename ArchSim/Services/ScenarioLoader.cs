using ArchSim.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace ArchSim.Services;

public class ScenarioLoader
{
    private readonly IMemoryCache _cache;
    private readonly string _scenariosPath;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ScenarioLoader(IMemoryCache cache, IWebHostEnvironment env, IConfiguration config)
    {
        _cache = cache;
        var configured = config["ScenariosPath"] ?? "Data/Scenarios";
        _scenariosPath = Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(env.ContentRootPath, configured);
    }

    public List<ScenarioSummary> GetAll()
    {
        return _cache.GetOrCreate("scenario_summaries", entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);
            var summaries = new List<ScenarioSummary>();

            if (!Directory.Exists(_scenariosPath))
                return summaries;

            foreach (var file in Directory.GetFiles(_scenariosPath, "*.json"))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var scenario = JsonSerializer.Deserialize<Scenario>(json, _jsonOptions);
                    if (scenario != null)
                    {
                        summaries.Add(new ScenarioSummary
                        {
                            Id = scenario.Id,
                            Title = scenario.Title,
                            Category = scenario.Category,
                            Difficulty = scenario.Difficulty,
                            EstimatedMinutes = scenario.EstimatedMinutes,
                            Description = scenario.Description,
                            Tags = scenario.Tags,
                            TotalSteps = scenario.Steps.Count,
                            Sequence = scenario.Sequence
                        });
                    }
                }
                catch { /* skip malformed files */ }
            }

            return summaries.OrderBy(s => s.Sequence).ThenBy(s => s.Title).ToList();
        })!;
    }

    public Scenario? GetById(string id)
    {
        var cacheKey = $"scenario_{id}";
        return _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);
            var file = Path.Combine(_scenariosPath, $"{id}.json");
            if (!File.Exists(file)) return null;

            var json = File.ReadAllText(file);
            return JsonSerializer.Deserialize<Scenario>(json, _jsonOptions);
        });
    }
}
