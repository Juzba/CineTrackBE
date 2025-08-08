using CineTrackBE.Models;
using CineTrackBE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController(IUserService userService) : Controller
    {
        private readonly IUserService _userService = userService;



        // INDEX //
        public async Task<IActionResult> Index() => View(await _userService.GetUsersList());


        // DETAILS //
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null) return NotFound();

            var user = await _userService.GetUser(id);

            if (user == null) return NotFound();

            return View(user);
        }



        // CREATE //
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id, Email, PhoneNumber, PasswordHash, EmailConfirmed")] User user)
        {
            if (ModelState.IsValid)
            {
                // user with this email exist
                if (_userService.AnyUserExists_Email(user.Email))
                {
                    ModelState.AddModelError("Email", "Zadejte platnou e-mailovou adresu.");
                    return View(user);
                }

                Add_Additional_UserParametrs(ref user);
                Add_Hash_To_UserPassword(ref user);

                await _userService.AddUser(user);
                await _userService.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }



        // EDIT //
        public async Task<IActionResult> Edit(string? id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userService.GetUser(id);

            if (user == null) return NotFound();

            user.PasswordHash = "******";

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id, Email, PhoneNumber, PasswordHash, EmailConfirmed")] User user)
        {
            if (id != user.Id) return NotFound();


            if (ModelState.IsValid)
            {
                // user with this email exist
                if (_userService.AnyUserExists_Email(user.Email))
                {
                    ModelState.AddModelError("Email", "Zadejte platnou e-mailovou adresu.");
                    return View(user);
                }


                var completeUser = await CompleteUseData(user);
                if (completeUser == null) return NotFound();

                try
                {
                    _userService.UpdateUser(completeUser);
                    await _userService.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_userService.AnyUserExists_Id(completeUser.Id)) return NotFound();
                    else throw;
                }

                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }



        // DELETE //
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null) return NotFound();

            var user = await _userService.GetUser(id);

            if (user == null) return NotFound();


            return View(user);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userService.GetUser(id);
            if (user != null)
            {
                _userService.RemoveUser(user);
            }

            await _userService.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



        private static void Add_Additional_UserParametrs(ref User user)
        {
            if (user == null) throw new ArgumentNullException("user is null!");

            user.UserName = user.Email;
            user.NormalizedUserName = user.Email!.ToUpper();
            user.NormalizedEmail = user.Email!.ToUpper();
        }


        private static void Add_Hash_To_UserPassword(ref User user)
        {
            if (user == null) throw new ArgumentNullException("user is null!");

            PasswordHasher<User> ph = new();
            user.PasswordHash = ph.HashPassword(null!, user.PasswordHash);
        }


        private async Task<User?> CompleteUseData(User inputUser)
        {
            if (inputUser == null) throw new NullReferenceException("user is null!");


            var user = await _userService.GetUser(inputUser.Id);

            if (user != null)
            {

                inputUser.UserName = inputUser.Email;
                inputUser.NormalizedUserName = inputUser.Email!.ToUpper();
                inputUser.NormalizedEmail = inputUser.Email!.ToUpper();


                inputUser.ConcurrencyStamp = user.ConcurrencyStamp;
                inputUser.SecurityStamp = user.SecurityStamp;


                if (inputUser.PasswordHash.Contains("******"))
                {
                    inputUser.PasswordHash = user.PasswordHash;
                }
                else
                {
                    Add_Hash_To_UserPassword(ref inputUser);
                }


                return inputUser;
            }

            return null;
        }
    }
}
