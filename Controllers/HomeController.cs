using Microsoft.AspNetCore.Mvc;
using SnackbarB2C2PI4_LeviFunk_ClassLibrary;
using SnackbarB2C2PI4_LeviFunk_MVC.Data;
using SnackbarB2C2PI4_LeviFunk_MVC.Models;
using System.Diagnostics;

namespace SnackbarB2C2PI4_LeviFunk_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApiService apiService;

        public HomeController(ILogger<HomeController> logger, ApiService _apiService)
        {
            _logger = logger;
            apiService = _apiService;
        }

        public async Task<IActionResult> Index()
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
    }
}