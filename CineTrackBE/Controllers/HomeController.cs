using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using CineTrackBE.Models.ViewModel;
using CineTrackBE.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CineTrackBE.Controllers
{
    public class HomeController(ILogger<HomeController> logger, UserManager<User> userManager) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;
        private readonly UserManager<User> _userManager = userManager;

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string input1)
        {

            PasswordHasher<User> hasher = new();
            var hashPass = hasher.HashPassword(null!, "123456");


            ViewBag.hash = hashPass;
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
