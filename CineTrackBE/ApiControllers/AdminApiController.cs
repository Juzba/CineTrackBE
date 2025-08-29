using CineTrackBE.AppServices;
using CineTrackBE.Models.DTO;
using CineTrackBE.Models.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.ApiControllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class AdminApiController(ILogger<AdminApiController> logger, IRepository<Film> filmRepository, IRepository<Genre> genreRepository, IRepository<FilmGenre> filmGenreRepository) : ControllerBase
{
    private readonly IRepository<Genre> _genreRepository = genreRepository;
    private readonly IRepository<FilmGenre> _filmGenreRepository = filmGenreRepository;
    private readonly IRepository<Film> _filmRepository = filmRepository;
    private ILogger<AdminApiController> _logger = logger;


    // ADD GENRE //
    [HttpPost("AddGenre")]
    public async Task<ActionResult> AddGenre(GenreDto genre)
    {

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Wrong GenreDto input model in AddGenre method!");
            return BadRequest(ModelState);
        }
        
        // Is Genre in db?
        var exist = await _genreRepository.GetList().AnyAsync(p => p.Name == genre.Name);
        if (exist)
        {
            _logger.LogInformation("Genre already Exist {GenreName}", genre.Name);
            return Conflict($"Genre '{genre.Name}' already Exist");
        }


        var newGenre = new Genre { Name = genre.Name };

        try
        {
            await _genreRepository.AddAsync(newGenre);
            await _genreRepository.SaveChangesAsync();
            _logger.LogInformation("Success add new genre: {GenreName}", genre.Name);
            return Ok();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while trying to save Genre {GenreName} to db!", genre.Name);
            return StatusCode(500, $"Error occurred while trying to save Genre to db {ex}");
        }
    }


    // DELETE GENRE //
    [HttpDelete("RemoveGenre/{id}")]
    public async Task<ActionResult> DeleteGenre(int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Invalid genre ID. ID must be greater than 0.");
            return BadRequest("Invalid genre ID. ID must be greater than 0.");
        }

        var genre = await _genreRepository.GetAsync_Id(id);
        if (genre == null)
        {
            _logger.LogWarning("Genre with Id {GenreId} not exist!", id);
            return NotFound($"Genre with Id '{id}' not exist!");
        }


        var exist = await _filmGenreRepository.GetList().AnyAsync(p => p.GenreId == id);
        if (exist)
        {
            _logger.LogWarning("Genre '{GenreName}' cannot be deleted because it is used!", genre.Name);
            return Conflict($"Genre '{genre.Name}' cannot be deleted because it is used!");
        }


        try
        {
            _genreRepository.Remove(genre);
            await _genreRepository.SaveChangesAsync();
            _logger.LogInformation("Genre '{GenreName}' successfully deleted!", genre.Name);
            return Ok();

        }
        catch (Exception)
        {
            _logger.LogError("Error occurred while trying to save Genre '{GenreName}' to db!", genre.Name);
            return StatusCode(500, "Error occurred while trying to save Genre to db");
        }
    }


    // EDIT GENRE //
    [HttpPut("EditGenre/{id}")]
    public async Task<ActionResult> PutGenre(int id, [FromBody] GenreDto genreDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Genre is not valid {ModelState}", ModelState);
            return BadRequest($"Genre is not valid {ModelState}");
        }
            
        if (id <= 0) 
        {
            _logger.LogWarning("Invalid genre ID. ID must be greater than 0.");
            return BadRequest("Invalid genre ID. ID must be greater than 0.");
        }

        // find genre with id in db
        var genre = await _genreRepository.GetAsync_Id(id);
        if (genre == null) 
        {
            _logger.LogWarning("Genre with Id {GenreId} not found!", id);
            return NotFound($"Genre with Id '{id}' not found!");
        }

        // name already reserved?
        var exist = await _genreRepository.GetList().AnyAsync(p => p.Name == genreDto.Name);
        if (exist)
        {
            _logger.LogWarning("Genre NAME '{GenreName}' already exist!", genreDto.Name);
            return Conflict($"Genre NAME '{genreDto.Name}' already exist!");
        }

        genre.Name = genreDto.Name;


        try
        {
            _genreRepository.Update(genre);
            await _genreRepository.SaveChangesAsync();

            _logger.LogInformation("Genre '{GenreName}' successfully updated!", genre.Name);
            return Ok();
        }
        catch (Exception)
        {
            _logger.LogError("Error occurred while trying to save Genre '{GenreName}' to db!", genre.Name);
            return StatusCode(500, "Error occurred while trying to save Genre to db");
        }
    }



    // Get all films //
    [HttpGet("AllFilms")]
    public async Task<ActionResult<IEnumerable<FilmDto>>> GetAllFilms()
    {
        var films = await _filmRepository
            .GetList()
            .Include(p => p.FilmGenres)
            .ThenInclude(p => p.Genre)
            .ToListAsync();

        var newFilms = films.Select(f => new FilmDto
        {
            Id = f.Id,
            Name = f.Name,
            Description = f.Description,
            Director = f.Director,
            ImageFileName = f.ImageFileName,
            ReleaseDate = f.ReleaseDate,
            Genres = f.FilmGenres?.Select(fg => new GenreDto { Id = fg.Genre.Id, Name = fg.Genre.Name }).ToList() ?? []
        });

        _logger.LogInformation("Retrieved {FilmCount} films from the database.", newFilms.Count());
        return Ok(newFilms);
    }



    // ADD FILM //
    [HttpPost("AddFilm")]
    public async Task<ActionResult> AddFilm(FilmDto film)
    {
        if (!ModelState.IsValid) 
        {
            _logger.LogWarning("Wrong FilmDto input model in AddFilm method! {ModelState}", ModelState);
            return BadRequest(ModelState); 
        }

        var exist = await _filmRepository.GetList().AnyAsync(p => p.Name == film.Name);
        if (exist) 
        { 
            _logger.LogInformation("Film already Exist {FilmName}", film.Name);
            return Conflict($"Film '{film.Name}' already Exist");
        }

        using var transaction = await _filmRepository.BeginTransactionAsync();
        try
        {
            var newFilm = new Film
            {
                Name = film.Name,
                Description = film.Description,
                Director = film.Director,
                ImageFileName = film.ImageFileName,
                ReleaseDate = film.ReleaseDate,
            };

            await _filmRepository.AddAsync(newFilm);
            await _filmRepository.SaveChangesAsync();

            var filmGenres = film.Genres.Select(p => new FilmGenre { FilmId = newFilm.Id, GenreId = p.Id });
            await _filmGenreRepository.AddRangeAsync(filmGenres);
            await _filmGenreRepository.SaveChangesAsync();

            await transaction.CommitAsync();
            _logger.LogInformation("Success add new film include genres: {FilmName}", film.Name);
            return Ok();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            _logger.LogError("Error occurred while trying to save Film '{FilmName}' to db!", film.Name);
            return StatusCode(500, "Error occurred while trying to save Film to db");
        }
    }


    // DELETE FILM //
    //[HttpDelete("RemoveGenre/{id}")]
    //public async Task<ActionResult<bool>> DeleteGenre(int id)
    //{
    //    if (id <= 0) return BadRequest("Invalid genre ID. ID must be greater than 0.");

    //    var genre = await _genreRepository.GetAsync_Id(id);
    //    if (genre == null) return NotFound($"Genre with Id '{id}' not exist!");


    //    var exist = await _filmGenreRepository.GetList().AnyAsync(p => p.GenreId == id);
    //    if (exist) return Conflict($"Genre '' cannot be deleted because it is used!");


    //    _genreRepository.Remove(genre);
    //    var success = await _genreRepository.SaveChangesAsync();
    //    if (!success) return StatusCode(500, "Error occurred while trying to save Genre to db");

    //    return Ok(success);
    //}


    //// EDIT FILM //
    //[HttpPut("EditGenre/{id}")]
    //public async Task<ActionResult> PutGenre(int id, [FromBody] GenreDto genreDto)
    //{
    //    if (!ModelState.IsValid) return BadRequest($"Genre is not valid {ModelState}");
    //    if (id <= 0) return BadRequest($"Invalid genre ID. ID must be greater than 0.");

    //    // find genre with id in db
    //    var genre = await _genreRepository.GetAsync_Id(id);
    //    if (genre == null) return NotFound($"Genre with Id '{id}' not found!");

    //    // name already reserved?
    //    var exist = await _genreRepository.GetList().AnyAsync(p => p.Name == genreDto.Name);
    //    if (exist) return Conflict($"Genre NAME '{genreDto.Name}' already exist!");


    //    genre.Name = genreDto.Name;
    //    _genreRepository.Update(genre);

    //    var success = await _genreRepository.SaveChangesAsync();
    //    if (!success) return StatusCode(500, "Error occurred while trying to save Genre to db");

    //    return Ok(success);
    //}






}
