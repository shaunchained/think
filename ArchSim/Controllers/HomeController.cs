using ArchSim.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArchSim.Controllers;

public class HomeController : Controller
{
    private readonly ScenarioLoader _loader;

    public HomeController(ScenarioLoader loader)
    {
        _loader = loader;
    }

    public IActionResult Index()
    {
        var scenarios = _loader.GetAll();
        return View(scenarios);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
