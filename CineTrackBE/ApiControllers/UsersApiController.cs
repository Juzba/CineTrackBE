using CineTrackBE.AppServices;
using CineTrackBE.Models.DTO;
using CineTrackBE.Models.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace CineTrackBE.ApiControllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class UsersApiController(IRepository<IdentityRole> roleRepository, IRepository<ApplicationUser> userRepository, IRepository<IdentityUserRole<string>> userRoleRepository, ILogger<UsersApiController> logger) : ControllerBase
{
    private readonly ILogger<UsersApiController> _logger = logger;
    private readonly IRepository<ApplicationUser> _userRepository = userRepository;
    private readonly IRepository<IdentityUserRole<string>> _userRoleRepository = userRoleRepository;
    private readonly IRepository<IdentityRole> _roleRepository = roleRepository;


    // GET ALL USERS //
    [HttpGet("AllUsers")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUser()
    {
        try
        {
            var usersWithRoles = await _userRepository.GetList()
                .Select(user => new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    NewPassword = null,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    EmailConfirmed = user.EmailConfirmed,
                    Roles = _userRoleRepository.GetList()
                        .Where(ur => ur.UserId == user.Id)
                        .Join(_roleRepository.GetList(),
                            ur => ur.RoleId,
                            r => r.Id,
                            (ur, r) => r.Name)
                        .ToList()
                }).ToListAsync();


            _logger.LogInformation("Retrieved {UserCount} films from the database.", usersWithRoles.Count);
            return Ok(usersWithRoles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while trying to retrieve Users from the database.");
            return StatusCode(500, "Error occurred while trying to retrieve Users from the database.");
        }
    }


    // ADD USER //
    [HttpPost("AddUser")]
    public async Task<ActionResult<UserDto>> AddUser(UserDto user)
    {

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Wrong User input model in AddUser method!");
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(user.NewPassword) || user.NewPassword.Length < 6)
        {
            _logger.LogWarning("Password must be at least 6 characters long!");
            return BadRequest("Password must be at least 6 characters long!");
        }

        // Additional password complexity checks can be added here



        // Is User in db?
        var exist = await _userRepository.GetList().AnyAsync(p => p.NormalizedEmail == user.Email.ToUpper());
        if (exist)
        {
            _logger.LogInformation("User with this Email already Exist {UserEmail}", user.Email);
            return Conflict($"User with this Email already Exist {user.Email}");
        }

        using var transaction = await _userRepository.BeginTransactionAsync();
        try
        {
            var newUser = new ApplicationUser
            {
                UserName = user.UserName,
                NormalizedUserName = user.UserName.ToUpper(),
                Email = user.Email,
                NormalizedEmail = user.Email.ToUpper(),
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(null!, user.NewPassword)
            };

            await _userRepository.AddAsync(newUser);
            await _userRepository.SaveChangesAsync();


            var rolesList = await _roleRepository.GetList().ToListAsync();
            var newUserRoles = user.Roles
                .Select(p => new IdentityUserRole<string>
                {
                    UserId = newUser.Id,
                    RoleId = rolesList.FirstOrDefault(r => r.Name == p)!.Id
                });

            await _userRoleRepository.AddRangeAsync(newUserRoles);

            await _userRepository.SaveChangesAsync();

            await transaction.CommitAsync();
            _logger.LogInformation("Success add new user: {UserEmail}", user.Email);


            var resultDto = new UserDto
            {
                Id = newUser.Id,
                UserName = newUser.UserName,
                Email = newUser.Email,
                PhoneNumber = newUser.PhoneNumber,
                EmailConfirmed = newUser.EmailConfirmed,
                NewPassword = null,
                Roles = user.Roles
            };



            return CreatedAtAction(nameof(resultDto), new { id = resultDto.Id }, resultDto);

        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error occurred while trying to save User '{UserEmail}' to db!", user.Email);
            return StatusCode(500, "Error occurred while trying to save User to db");
        }
    }


    // DELETE USER //
    [HttpDelete("RemoveUser/{id}")]
    public async Task<ActionResult> DeleteUser(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            _logger.LogWarning("Invalid user ID. ID cannot be empty.");
            return BadRequest("Invalid user ID. ID cannot be empty.");
        }

        using var transaction = await _userRepository.BeginTransactionAsync();
        try
        {
            var user = await _userRepository.GetList()
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (user == null)
            {
                _logger.LogWarning("User with Id {UserId} not exist!", id);
                return NotFound($"User with Id '{id}' not exist!");
            }

            if (user.Email == User.FindFirstValue(ClaimTypes.Name))
            {
                _logger.LogWarning("User with Id {UserId} is trying to delete himself!", id);
                return BadRequest("You cannot delete your own account while logged in!");
            }

            // Remove associated userRoles
            var userRoles = await _userRoleRepository.GetList()
           .Where(ur => ur.UserId == id)
           .ToListAsync();

            if (userRoles.Count != 0)
            {
                _userRoleRepository.RemoveRange(userRoles);
                await _userRepository.SaveChangesAsync();
            }

            // Remove user 
            _userRepository.Remove(user);
            await _userRepository.SaveChangesAsync();
            await transaction.CommitAsync();


            _logger.LogInformation("User '{UserEmail}' successfully deleted!", user.Email);
            return Ok();

        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error occurred while deleting user {UserId}", id);
            return StatusCode(500, "Error occurred while deleting user");
        }
    }


    // EDIT USER //
    [HttpPut("EditUser/{id}")]
    public async Task<ActionResult<UserDto>> PutUser(string id, [FromBody] UserDto userDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("User is not valid {ModelState}", ModelState);
            return BadRequest($"User is not valid {ModelState}");
        }

        if (!string.IsNullOrWhiteSpace(userDto.NewPassword))
        {
            // Validate password is not in model
            if (userDto.NewPassword.Length < 6)
            {
                _logger.LogWarning("Password must be at least 6 characters long!");
                return BadRequest("Password must be at least 6 characters long!");
            }
            // Additional password complexity checks can be added here

        }

        if (string.IsNullOrEmpty(id))
        {
            _logger.LogWarning("Invalid user ID. ID must be greater than 0.");
            return BadRequest("Invalid user ID. ID must be greater than 0.");
        }

        // find user with id in db
        var user = await _userRepository.GetAsync_Id(id);
        if (user == null)
        {
            _logger.LogWarning("User with Id {UserId} not found!", id);
            return NotFound($"User with Id '{id}' not found!");
        }

        // Email already reserved on diferent User?
        var exist = await _userRepository.GetList().AnyAsync(p => p.NormalizedEmail == userDto.Email.ToUpper() && p.Id != userDto.Id);
        if (exist)
        {
            _logger.LogInformation("User with this Email already Exist {UserEmail}", userDto.Email);
            return Conflict($"User with this Email already Exist {userDto.Email}");
        }

        using var transaction = await _userRepository.BeginTransactionAsync();
        try
        {

            user.Email = userDto.Email;
            user.NormalizedEmail = userDto.Email.ToUpper();
            user.UserName = userDto.UserName;
            user.NormalizedUserName = userDto.UserName.ToUpper();
            user.PhoneNumber = userDto.PhoneNumber;
            user.EmailConfirmed = userDto.EmailConfirmed;
            if (!string.IsNullOrWhiteSpace(userDto.NewPassword))
            {
                user.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(null!, userDto.NewPassword);
            }

            // Remove old userRoles
            var oldUserRoles = await _userRoleRepository.GetList().Where(ur => ur.UserId == user.Id).ToListAsync();
            if (oldUserRoles.Count > 0)
            {
                _userRoleRepository.RemoveRange(oldUserRoles);
            }

            // Add new userRoles
            var rolesList = await _roleRepository.GetList().ToListAsync();
            var Roles = userDto.Roles
                .Select(ur => rolesList.FirstOrDefault(r => r.Name == ur))
                .Where(r => r != null);

            var newUserRoles = Roles.Select(r => new IdentityUserRole<string>
            {
                UserId = user.Id,
                RoleId = r.Id
            }).ToList();

            await _userRoleRepository.AddRangeAsync(newUserRoles);



            await _userRepository.SaveChangesAsync();
            await transaction.CommitAsync();
            _logger.LogInformation("User '{UserEmail}' successfully updated!", user.Email);



            var newUserDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                NewPassword = null,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                Roles = userDto.Roles
            };

            return Ok(newUserDto);

        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error occurred while trying to update User '{UserEmail}' in db!", user.Email);
            return StatusCode(500, "Error occurred while trying to update User in db");
        }
    }

}
