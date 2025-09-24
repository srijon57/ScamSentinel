using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ScamSentinel.Models;

namespace ScamSentinel.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult About()
        {
            return View();
        }
        public IActionResult Legality()
        {
            return View();
        }
        public IActionResult StartupTipsResults()
        {
            // Pass query parameters to view
            ViewBag.BusinessType = HttpContext.Request.Query["businessType"];
            ViewBag.FormData = HttpContext.Request.Query;
            return View();
        }
    }
}
