using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using thinking1.Models;
using thinking1.Services;

namespace thinking1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ScenarioService _scenarioService;

        public HomeController(ScenarioService scenarioService)
        {
            _scenarioService = scenarioService;
        }

        public IActionResult Index()
        {
            var summaries = _scenarioService.GetAllSummaries();
            return View(summaries);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
