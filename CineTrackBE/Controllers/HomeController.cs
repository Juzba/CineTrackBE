using CineTrackBE.Models.Entities;
using CineTrackBE.Models.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CineTrackBE.Controllers
{
    public class HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string input1)
        {

            PasswordHasher<ApplicationUser> hasher = new();
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
