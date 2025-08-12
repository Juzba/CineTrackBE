using CineTrackBE.Models.Entities;
using CineTrackBE.Models.ViewModel;
using CineTrackBE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.Areas.AdminArea.Controllers
{
    [Area("AdminArea")]
    [Authorize(Roles = "Admin")]
    public class UsersController(IUserService userService, IRepository<User> userRepository, IRepository<IdentityRole> roleRepository, IRepository<IdentityUserRole<string>> userRoleRepository, IRoleService roleService) : Controller
    {
        private readonly IRepository<User> _userRepository = userRepository;
        private readonly IRepository<IdentityRole> _roleRepository = roleRepository;
        private readonly IRepository<IdentityUserRole<string>> _userRoleRepository = userRoleRepository;
        private readonly IUserService _userService = userService;
        private readonly IRoleService _roleService = roleService;

        const string AdminConst = "Admin";
        const string UserConst = "User";




        // INDEX //
        public async Task<IActionResult> Index()
        {
            var users = await _userRepository.GetList().ToListAsync();
            var roles = await _roleRepository.GetList().ToListAsync();
            var userRoles = await _userRoleRepository.GetList().ToListAsync();



            var userWithRolesList = users.Select(p => new UserWithRoles()
            {
                Id = p.Id,
                EmailConfirmed = p.EmailConfirmed,
                PhoneNumber = p.PhoneNumber,
                UserName = p.UserName,
                Roles = []
            }).ToList();


            foreach (var userRole in userRoles)
            {
                var roleName = roles.FirstOrDefault(p => p.Id == userRole.RoleId)?.Name;

                userWithRolesList.FirstOrDefault(p => p.Id == userRole.UserId)?.Roles.Add(roleName);
            }


            return View(userWithRolesList);
        }


        // DETAILS //
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null) return NotFound();

            var user = await _userRepository.GetAsync_Id(id);

            if (user == null) return NotFound();

            var roles = await _roleService.GetRolesFromUserAsync(user);


            var userWithRoles = new UserWithRoles()
            {
                UserName = user.UserName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                Id = user.Id,
                PhoneNumber = user.PhoneNumber,
                Roles = [.. roles.Select(p => p.Name)]
            };


            return View(userWithRoles);
        }


        // CREATE //
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id, UserName, PhoneNumber, PasswordHash, EmailConfirmed")] User user, bool roleUser, bool roleAdmin)
        {
            if (ModelState.IsValid)
            {
                // user with this UserName exist
                if (await _userService.AnyUserExistsByUserNameAsync(user.UserName))
                {
                    ModelState.AddModelError("UserName", "UserName je obsazen!");
                    return View(user);
                }

                Add_Additional_UserParametrs(ref user);
                Add_Hash_To_UserPassword(ref user);


                // add role admin
                if (roleAdmin) await _roleService.AddUserRoleAsync(user, AdminConst);
                // add  role user
                if (roleUser) await _roleService.AddUserRoleAsync(user, UserConst);


                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }



        // EDIT //
        public async Task<IActionResult> Edit(string? id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userRepository.GetAsync_Id(id);

            if (user == null) return NotFound();

            var roles = await _roleService.GetRolesFromUserAsync(user);

            var userWithRoles = new UserWithRoles()
            {
                Id = user.Id,
                EmailConfirmed = user.EmailConfirmed,
                PasswordHash = "******",
                PhoneNumber = user.PhoneNumber,
                UserName = user.UserName,
                Roles = roles.Select(p => p.Name).ToList()
            };

            return View(userWithRoles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id, UserName, PhoneNumber, PasswordHash, EmailConfirmed")] UserWithRoles formUser, bool roleAdmin, bool roleUser)
        {
            if (id != formUser.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var defaultUser = await _userRepository.GetAsync_Id(id);
                if (defaultUser == null) return NotFound();

                // user with this UserName exist?
                if (defaultUser.UserName != formUser.UserName && await _userService.AnyUserExistsByUserNameAsync(formUser.UserName))
                {
                    ModelState.AddModelError("UserName", "User je obsazen.");
                    return View(formUser);
                }

                var completedUser = CompleteUseData(formUser, defaultUser);
                if (completedUser == null) return NotFound();

                // add or remove role admin
                if (roleAdmin) await _roleService.AddUserRoleAsync(completedUser, AdminConst);
                else if (await _roleService.CountUserInRoleAsync(AdminConst) >= 2) await _roleService.RemoveUserRoleAsync(completedUser, AdminConst);
                else TempData["info"] = "Nelze odebrat posledního Admina!!";

                // add or remove role user
                if (roleUser) await _roleService.AddUserRoleAsync(completedUser, UserConst);
                else await _roleService.RemoveUserRoleAsync(completedUser, UserConst);

                await _userRepository.SaveChangesAsync();

                try
                {
                    _userRepository.Update(completedUser);
                    await _userRepository.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _userRepository.AnyExistsAsync(completedUser.Id)) return NotFound();
                    else throw;
                }

                return RedirectToAction(nameof(Index));
            }
            return View(formUser);
        }



        // DELETE //
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null) return NotFound();

            var user = await _userRepository.GetAsync_Id(id);

            if (user == null) return NotFound();

            return View(user);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userRepository.GetAsync_Id(id);
            if (user != null)
            {
                var roles = await _roleService.GetRolesFromUserAsync(user);
                var adminsCount = await _roleService.CountUserInRoleAsync(AdminConst);

                // Last Admin cannot be removed
                if (roles.Any(p => p.Name == "Admin") && adminsCount <= 1)
                {
                    TempData["info"] = "Nelze smazat posledního Admina!!";
                    return RedirectToAction(nameof(Index));
                }

                 _userRepository.Remove(user);
            }

            await _userRepository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



        private static void Add_Additional_UserParametrs(ref User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            user.Email = user.UserName;
            user.NormalizedUserName = user.UserName!.ToUpper();
            user.NormalizedEmail = user.UserName!.ToUpper();
        }


        private static void Add_Hash_To_UserPassword(ref User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            PasswordHasher<User> ph = new();
            user.PasswordHash = ph.HashPassword(null!, user.PasswordHash);
        }


        private User? CompleteUseData(UserWithRoles formUser, User defaultUser)
        {
            ArgumentNullException.ThrowIfNull(formUser);
            ArgumentNullException.ThrowIfNull(defaultUser);


            if (defaultUser != null)
            {

                defaultUser.UserName = formUser.UserName;
                defaultUser.Email = formUser.UserName;
                defaultUser.NormalizedUserName = formUser.UserName!.ToUpper();
                defaultUser.NormalizedEmail = formUser.UserName!.ToUpper();
                defaultUser.PhoneNumber = formUser.PhoneNumber;
                defaultUser.EmailConfirmed = formUser.EmailConfirmed;


                if (!formUser.PasswordHash.Contains("******"))
                {
                    defaultUser.PasswordHash = formUser.PasswordHash;
                    Add_Hash_To_UserPassword(ref defaultUser);
                }


                return defaultUser;
            }

            return null;
        }
    }
}