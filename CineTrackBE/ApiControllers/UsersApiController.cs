using CineTrackBE.AppServices;
using CineTrackBE.Models.DTO;
using CineTrackBE.Models.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.ApiControllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class UsersApiController(IRepository<IdentityRole<string>> roleRepository, IRepository<ApplicationUser> userRepository, IRepository<IdentityUserRole<string>> userRoleRepository, ILogger<UsersApiController> logger) : ControllerBase
{
    private readonly ILogger<UsersApiController> _logger = logger;
    private readonly IRepository<ApplicationUser> _userRepository = userRepository;
    private readonly IRepository<IdentityUserRole<string>> _userRoleRepository = userRoleRepository;
    private readonly IRepository<IdentityRole<string>> _roleRepository = roleRepository;


    // GET ALL USERS //
    [HttpGet("AllUsers")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUser()
    {
        try
        {
            var users = await _userRepository.GetList().ToListAsync();

            var newUserDto = users.Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                NewPassword = null,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                EmailConfirmed = u.EmailConfirmed,
            });

      

            _logger.LogInformation("Retrieved {UserCount} films from the database.", newUserDto.Count());
            return Ok(newUserDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while trying to retrieve Users from the database.");
            return StatusCode(500, "Error occurred while trying to retrieve Users from the database.");
        }
    }


    // ADD GENRE //
    //[HttpPost("AddUser")]
    //public async Task<ActionResult<GenreDto>> AddUser(GenreDto genre)
    //{

    //    //if (!ModelState.IsValid)
    //    //{
    //    //    _logger.LogWarning("Wrong GenreDto input model in AddGenre method!");
    //    //    return BadRequest(ModelState);
    //    //}

    //    //// Is Genre in db?
    //    //var exist = await _genreRepository.GetList().AnyAsync(p => p.Name == genre.Name);
    //    //if (exist)
    //    //{
    //    //    _logger.LogInformation("Genre already Exist {GenreName}", genre.Name);
    //    //    return Conflict($"Genre '{genre.Name}' already Exist");
    //    //}


    //    //var newGenre = new Genre { Name = genre.Name };

    //    //try
    //    //{
    //    //    await _genreRepository.AddAsync(newGenre);
    //    //    await _genreRepository.SaveChangesAsync();
    //    //    _logger.LogInformation("Success add new genre: {GenreName}", genre.Name);


    //    //    var resultDto = new GenreDto { Id = newGenre.Id, Name = newGenre.Name };
    //    //    return CreatedAtAction(nameof(AddGenre), new { id = resultDto.Id }, resultDto);

    //    //}
    //    //catch (Exception ex)
    //    //{
    //    //    _logger.LogError(ex, "Error occurred while trying to save Genre {GenreName} to db!", genre.Name);
    //    //    return StatusCode(500, $"Error occurred while trying to save Genre to db {ex}");
    //    //}
    //}


    //// DELETE GENRE //
    //[HttpDelete("RemoveGenre/{id}")]
    //public async Task<ActionResult> DeleteGenre(int id)
    //{
    //    //if (id <= 0)
    //    //{
    //    //    _logger.LogWarning("Invalid genre ID. ID must be greater than 0.");
    //    //    return BadRequest("Invalid genre ID. ID must be greater than 0.");
    //    //}

    //    //var genre = await _genreRepository.GetAsync_Id(id);
    //    //if (genre == null)
    //    //{
    //    //    _logger.LogWarning("Genre with Id {GenreId} not exist!", id);
    //    //    return NotFound($"Genre with Id '{id}' not exist!");
    //    //}


    //    //var exist = await _filmGenreRepository.GetList().AnyAsync(p => p.GenreId == id);
    //    //if (exist)
    //    //{
    //    //    _logger.LogWarning("Genre '{GenreName}' cannot be deleted because it is used!", genre.Name);
    //    //    return Conflict($"Genre '{genre.Name}' cannot be deleted because it is used!");
    //    //}


    //    //try
    //    //{
    //    //    _genreRepository.Remove(genre);
    //    //    await _genreRepository.SaveChangesAsync();
    //    //    _logger.LogInformation("Genre '{GenreName}' successfully deleted!", genre.Name);
    //    //    return Ok();

    //    //}
    //    //catch (Exception)
    //    //{
    //    //    _logger.LogError("Error occurred while trying to save Genre '{GenreName}' to db!", genre.Name);
    //    //    return StatusCode(500, "Error occurred while trying to save Genre to db");
    //    //}
    //}


    //// EDIT GENRE //
    //[HttpPut("EditGenre/{id}")]
    //public async Task<ActionResult<GenreDto>> PutGenre(int id, [FromBody] GenreDto genreDto)
    //{
    //    //if (!ModelState.IsValid)
    //    //{
    //    //    _logger.LogWarning("Genre is not valid {ModelState}", ModelState);
    //    //    return BadRequest($"Genre is not valid {ModelState}");
    //    //}

    //    //if (id <= 0)
    //    //{
    //    //    _logger.LogWarning("Invalid genre ID. ID must be greater than 0.");
    //    //    return BadRequest("Invalid genre ID. ID must be greater than 0.");
    //    //}

    //    //// find genre with id in db
    //    //var genre = await _genreRepository.GetAsync_Id(id);
    //    //if (genre == null)
    //    //{
    //    //    _logger.LogWarning("Genre with Id {GenreId} not found!", id);
    //    //    return NotFound($"Genre with Id '{id}' not found!");
    //    //}

    //    //// name already reserved?
    //    //var exist = await _genreRepository.GetList().AnyAsync(p => p.Name == genreDto.Name);
    //    //if (exist)
    //    //{
    //    //    _logger.LogWarning("Genre NAME '{GenreName}' already exist!", genreDto.Name);
    //    //    return Conflict($"Genre NAME '{genreDto.Name}' already exist!");
    //    //}

    //    //genre.Name = genreDto.Name;


    //    //try
    //    //{
    //    //    _genreRepository.Update(genre);
    //    //    await _genreRepository.SaveChangesAsync();

    //    //    _logger.LogInformation("Genre '{GenreName}' successfully updated!", genre.Name);
    //    //    return Ok(new GenreDto { Id = genre.Id, Name = genre.Name });
    //    //}
    //    //catch (Exception)
    //    //{
    //    //    _logger.LogError("Error occurred while trying to save Genre '{GenreName}' to db!", genre.Name);
    //    //    return StatusCode(500, "Error occurred while trying to save Genre to db");
    //    //}
    //}
























}
