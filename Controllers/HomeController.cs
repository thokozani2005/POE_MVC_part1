using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using POE_MVC_part1.Models;

namespace POE_MVC_part1.Controllers
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
            return RedirectToAction("Login");
        }
        public IActionResult About()
        {
            return View();
        }
        public IActionResult contact()
        {
            return View();
        }
        public IActionResult LogIn()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
