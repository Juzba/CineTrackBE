using CineTrackBE.AppServices;
using CineTrackBE.Models.Entities;
using CineTrackBE.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController(ILogger<UsersController> logger, IRepository<ApplicationUser> userRepository, IRepository<IdentityRole> roleRepository, IRepository<IdentityUserRole<string>> userRoleRepository, IDataService dataService) : Controller
    {
        private readonly ILogger<UsersController> _logger = logger;
        private readonly IRepository<ApplicationUser> _userRepository = userRepository;
        private readonly IRepository<IdentityRole> _roleRepository = roleRepository;
        private readonly IRepository<IdentityUserRole<string>> _userRoleRepository = userRoleRepository;
        private readonly IDataService _dataService = dataService;


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
            if (id == null)
            {
                _logger.LogWarning("Details action called with null ID.");
                return NotFound();
            }

            var user = await _userRepository.GetAsync(id);

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found in Details action.", id);
                return NotFound();
            }

            var roles = await _dataService.GetRolesFromUserAsync(user);


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
        public async Task<IActionResult> Create([Bind("Id, UserName, PhoneNumber, PasswordHash, EmailConfirmed")] ApplicationUser user, bool roleUser, bool roleAdmin)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("User creation failed due to invalid model state.");
                return View(user);
            }

            // user with this UserName exist
            if (await _dataService.AnyUserExistsByUserNameAsync(user.UserName))
            {
                ModelState.AddModelError("UserName", "UserName je obsazen!");
                _logger.LogWarning("User creation failed because the username {UserName} is already taken.", user.UserName);
                return View(user);
            }

            Add_Additional_UserParametrs(ref user);
            Add_Hash_To_UserPassword(ref user);


            try
            {
                // add role admin
                if (roleAdmin) await _dataService.AddUserRoleAsync(user, AdminConst);
                // add  role user
                if (roleUser) await _dataService.AddUserRoleAsync(user, UserConst);


                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();
                _logger.LogInformation("New user created with ID {UserId}", user.Id);
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new user.");
                ModelState.AddModelError(string.Empty, "Něco se pokazilo, zkuste to prosím znovu.");
                return View(user);
            }
        }



        // EDIT //
        public async Task<IActionResult> Edit(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Edit action called with null or empty ID.");
                return NotFound();
            }

            var user = await _userRepository.GetAsync(id);

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found in Edit action.", id);
                return NotFound();
            }

            var roles = await _dataService.GetRolesFromUserAsync(user);

            var userWithRoles = new UserWithRoles()
            {
                Id = user.Id,
                EmailConfirmed = user.EmailConfirmed,
                PasswordHash = "******",
                PhoneNumber = user.PhoneNumber,
                UserName = user.UserName,
                Roles = [.. roles.Select(p => p.Name)]
            };

            return View(userWithRoles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id, UserName, PhoneNumber, PasswordHash, EmailConfirmed")] UserWithRoles formUser, bool roleAdmin, bool roleUser)
        {
            if (id != formUser.Id)
            {
                _logger.LogWarning("User edit failed due to ID mismatch.");
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("User edit failed due to invalid model state.");
                return View(formUser);
            }

            var defaultUser = await _userRepository.GetAsync(id);
            if (defaultUser == null)
            {
                _logger.LogWarning("User with ID {UserId} not found for editing.", id);
                return NotFound();
            }

            // user with this UserName exist?
            if (defaultUser.UserName != formUser.UserName && await _dataService.AnyUserExistsByUserNameAsync(formUser.UserName))
            {
                _logger.LogWarning("User edit failed because the username {UserName} is already taken.", formUser.UserName);
                ModelState.AddModelError("UserName", "User je obsazen.");
                return View(formUser);
            }

            var completedUser = CompleteUseData(formUser, defaultUser);
            if (completedUser == null)
            {
                _logger.LogWarning("Failed to complete user data for user ID {UserId}.", id);
                return NotFound();
            }

            using var transaction = await _userRepository.BeginTransactionAsync();
            try
            {

                var userRoles = await _dataService.GetRolesFromUserAsync(defaultUser);


                // add or remove role admin
                if (roleAdmin && !userRoles.Any(p => p.Name == "Admin"))
                {
                    await _dataService.AddUserRoleAsync(completedUser, AdminConst);
                }
                else if (!roleAdmin && userRoles.Any(p => p.Name == "Admin"))
                {
                    if (await _dataService.CountUserInRoleAsync(AdminConst) >= 2)
                    {
                        await _dataService.RemoveUserRoleAsync(completedUser, AdminConst);
                    }
                    else
                    {
                        _logger.LogWarning("Attempt to remove the last Admin role from user ID {UserId} was blocked.", formUser.Id);
                        TempData["info"] = "Nelze odebrat posledního Admina!!";
                    }
                }

                // add or remove role user
                if (roleUser && !userRoles.Any(p => p.Name == "User"))
                {
                    await _dataService.AddUserRoleAsync(completedUser, UserConst);
                }
                else if (!roleUser && userRoles.Any(p => p.Name == "User"))
                {
                    await _dataService.RemoveUserRoleAsync(completedUser, UserConst);
                }

                await _userRepository.SaveChangesAsync();

                await _userRepository.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("User with ID {UserId} successfully edited.", formUser.Id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while editing user with ID {UserId}.", formUser.Id);
                ModelState.AddModelError(string.Empty, "Něco se pokazilo, zkuste to prosím znovu.");
                return View(formUser);
            }

        }



        // DELETE //
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Delete action called with null ID.");
                return NotFound();
            }

            var user = await _userRepository.GetAsync(id);

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found in Delete action.", id);
                return NotFound();
            }

            return View(user);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("User deletion failed due to null or empty ID.");
                return NotFound();
            }


            var user = await _userRepository.GetAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found for deletion.", id);
                return NotFound();
            }

            var roles = await _dataService.GetRolesFromUserAsync(user);
            var adminsCount = await _dataService.CountUserInRoleAsync(AdminConst);

            // Last Admin cannot be removed
            if (roles.Any(p => p.Name == "Admin") && adminsCount <= 1)
            {
                TempData["info"] = "Nelze smazat posledního Admina!!";
                _logger.LogWarning("Attempt to delete the last Admin user with ID {UserId} was blocked.", id);
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _userRepository.Remove(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation("User with ID {UserId} successfully deleted.", id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting user with ID {UserId}.", id);
                ModelState.AddModelError(string.Empty, "Něco se pokazilo, zkuste to prosím znovu.");
                return View(user);
            }

        }



        private static void Add_Additional_UserParametrs(ref ApplicationUser user)
        {
            ArgumentNullException.ThrowIfNull(user);

            user.Email = user.UserName;
            user.NormalizedUserName = user.UserName!.ToUpper();
            user.NormalizedEmail = user.UserName!.ToUpper();
        }


        private static void Add_Hash_To_UserPassword(ref ApplicationUser user)
        {
            ArgumentNullException.ThrowIfNull(user);

            PasswordHasher<ApplicationUser> ph = new();
            user.PasswordHash = ph.HashPassword(null!, user.PasswordHash);
        }


        private ApplicationUser? CompleteUseData(UserWithRoles formUser, ApplicationUser defaultUser)
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