using CineTrackBE.AppServices;
using CineTrackBE.Models.DTO;
using CineTrackBE.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CineTrackBE.ApiControllers;

[Route("api/[controller]")]
[ApiController]
public class AuthApiController(ILogger<AuthApiController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IJwtService jwtService) : ControllerBase
{
    private readonly ILogger<AuthApiController> _logger = logger;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IJwtService _jwtService = jwtService;


    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> LoginAsync([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid login model state");
            return BadRequest(ModelState);
        }

        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            _logger.LogWarning("Login attempt failed for non-existent user: {UserEmail}", loginDto.Email);
            return Unauthorized();
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Invalid password attempt for user: {UserEmail}", loginDto.Email);
            return Unauthorized();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user, roles);
        var userDto = new UserDto { UserName = user.UserName, Email = user.Email ?? "", Roles = roles };

        _logger.LogInformation("User {UserEmail} logged in successfully", loginDto.Email);
        return Ok(new AuthResponseDto { Token = token, User = userDto });
    }



    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto registerData)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid registration model state: {ModelState}",ModelState);
            return BadRequest(ModelState);
        }

        var existingUser = await _userManager.FindByEmailAsync(registerData.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Registration attempt with existing username: {UserEmail}", registerData.Email);
            return Conflict("User with this Email already exists");
        }

        var user = new ApplicationUser
        {
            UserName = registerData.Email,
            Email = registerData.Email
        };

        var result = await _userManager.CreateAsync(user, registerData.Password);
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to create user: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return BadRequest(result.Errors);
        }

        _logger.LogInformation("User {UserEmail} registered successfully", registerData.Email);
        return CreatedAtAction(nameof(LoginAsync), new { username = user.UserName }, null);
    }



    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = new();
    }
}
