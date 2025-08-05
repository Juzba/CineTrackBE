using CineTrackBE.Data;
using CineTrackBE.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CineTrackBE.Controllers
{
    public class HomeController(ILogger<HomeController> logger, ApplicationDbContext context) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;
        private readonly ApplicationDbContext _context = context;

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string input1)
        {
       

            var user = _context.Users.FirstOrDefault(p => p.Email == "Karel@gmail.com");

            if(user == null) return View();


            PasswordHasher<IdentityUser> hasher = new();
            var hashPass = hasher.HashPassword(user, "123456");

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
