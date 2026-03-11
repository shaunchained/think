using System.Text.Json;
using thinking1.Models;

namespace thinking1.Services
{
    public class ScenarioService
    {
        private readonly List<FullScenario> _scenarios;
        private readonly JsonSerializerOptions _jsonOptions;

        public ScenarioService(IConfiguration configuration, IWebHostEnvironment env, ILogger<ScenarioService> logger)
        {
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            _scenarios = new List<FullScenario>();

            var scenariosPath = configuration["ScenariosPath"] ?? "Data/scenarios";
            var fullPath = Path.Combine(env.ContentRootPath, scenariosPath);

            if (!Directory.Exists(fullPath))
            {
                logger.LogWarning("Scenarios directory not found: {Path}", fullPath);
                return;
            }

            var files = Directory.GetFiles(fullPath, "*.json");
            foreach (var file in files)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var scenario = JsonSerializer.Deserialize<FullScenario>(json, _jsonOptions);
                    if (scenario != null)
                    {
                        _scenarios.Add(scenario);
                        logger.LogInformation("Loaded scenario: {Id}", scenario.Id);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to load scenario file: {File}", file);
                }
            }
        }

        public List<ScenarioSummary> GetAllSummaries()
        {
            return _scenarios
                .Select(s => new ScenarioSummary
                {
                    Id = s.Id,
                    Title = s.Title,
                    Description = s.Description,
                    Type = s.Type,
                    DifficultyLabel = s.Difficulty,
                    EstimatedMinutes = s.EstimatedMinutes,
                    TotalQuestions = s.Steps.Count,
                    IsAvailable = s.IsAvailable,
                    Sequence = s.Sequence
                })
                .OrderBy(s => s.Sequence)
                .ThenBy(s => s.Title)
                .ToList();
        }

        public FullScenario? GetById(string id)
        {
            return _scenarios.FirstOrDefault(s => s.Id == id);
        }
    }
}
