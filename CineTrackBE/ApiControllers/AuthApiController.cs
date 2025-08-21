

using CineTrackBE.AppServices;
using CineTrackBE.Models.DTO;
using CineTrackBE.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CineTrackBE.ApiControllers;

[Route("api/[controller]")]
[ApiController]
public class AuthApiController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IJwtService jwtService) : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IJwtService _jwtService = jwtService;


    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginDto loginDto)
    {
        if (string.IsNullOrWhiteSpace(loginDto.UserName) || string.IsNullOrWhiteSpace(loginDto.Password))
        {
            return BadRequest("UserName and Password are required!");
        }

        var user = await _userManager.FindByNameAsync(loginDto.UserName);
        if (user == null) return Unauthorized();


        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

        if (!result.Succeeded) return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user, roles);

        var userDto = new UserDto { UserName = user.UserName, Email = user.Email ?? "", Roles = roles };

        return Ok(new { Token = token, User = userDto });
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto registerData)
    {
        if (string.IsNullOrWhiteSpace(registerData.UserName) || string.IsNullOrWhiteSpace(registerData.Password))
        {
            return BadRequest("UserName and Password are required!");
        }

        var user = await _userManager.FindByNameAsync(registerData.UserName);

        if (user != null) return Conflict("User with this UserName already exists!");

        user = new ApplicationUser
        {
            UserName = registerData.UserName,
            Email = registerData.UserName,
            NormalizedEmail = registerData.UserName.ToUpper(),
            NormalizedUserName = registerData.UserName.ToUpper(),
            PhoneNumber = null,
            PasswordHash = _userManager.PasswordHasher.HashPassword(new ApplicationUser(), registerData.Password),
        };

        _userManager.CreateAsync(user).GetAwaiter().GetResult();

        return Created("User registered successfully!", null);
    }

}
