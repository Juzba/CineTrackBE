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
public class AdminApiController(IRepository<IdentityUserRole<string>> userRoleRepository, ILogger<AdminApiController> logger, IRepository<Film> filmRepository, IRepository<Genre> genreRepository, IRepository<FilmGenre> filmGenreRepository) : ControllerBase
{
    private readonly IRepository<Genre> _genreRepository = genreRepository;
    private readonly IRepository<FilmGenre> _filmGenreRepository = filmGenreRepository;
    private readonly IRepository<Film> _filmRepository = filmRepository;
    private readonly IRepository<IdentityUserRole<string>> _userRoleRepository = userRoleRepository;
    private readonly ILogger<AdminApiController> _logger = logger;


    // ADD GENRE //
    [HttpPost("AddGenre")]
    public async Task<ActionResult<GenreDto>> AddGenre(GenreDto genre)
    {

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Wrong GenreDto input model in AddGenre method!");
            return BadRequest(ModelState);
        }


        try
        {
            // Is Genre in db?
            var exist = await _genreRepository.AnyAsync(p => p.Name == genre.Name);
            if (exist)
            {
                _logger.LogInformation("Genre already Exist {GenreName}", genre.Name);
                return Conflict($"Genre '{genre.Name}' already Exist");
            }


            var newGenre = new Genre { Name = genre.Name };
            await _genreRepository.AddAsync(newGenre);
            await _genreRepository.SaveChangesAsync();
            _logger.LogInformation("Success add new genre: {GenreName}", genre.Name);


            var resultDto = new GenreDto { Id = newGenre.Id, Name = newGenre.Name };
            return CreatedAtAction(nameof(AddGenre), new { id = resultDto.Id }, resultDto);

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

        try
        {
            var genre = await _genreRepository.GetAsync(id);
            if (genre == null)
            {
                _logger.LogWarning("Genre with Id {GenreId} not exist!", id);
                return NotFound($"Genre with Id '{id}' not exist!");
            }


            var isGenreUsed = await _filmGenreRepository.GetList().AnyAsync(p => p.GenreId == id);
            if (isGenreUsed)
            {
                _logger.LogWarning("Genre '{GenreName}' cannot be deleted because it is used!", genre.Name);
                return Conflict($"Genre '{genre.Name}' cannot be deleted because it is used!");
            }

            _genreRepository.Remove(genre);
            await _genreRepository.SaveChangesAsync();
            _logger.LogInformation("Genre '{GenreName}' successfully deleted!", genre.Name);
            return Ok($"Genre '{genre.Name}' deleted");

        }
        catch (Exception)
        {
            _logger.LogError("Error occurred while trying to save Genre to db!");
            return StatusCode(500, "Error occurred while trying to save Genre to db");
        }
    }


    // EDIT GENRE //
    [HttpPut("EditGenre/{id}")]
    public async Task<ActionResult<GenreDto>> EditGenre(int id, [FromBody] GenreDto genreDto)
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

        try
        {
            // find genre with id in db
            var genre = await _genreRepository.GetAsync(id);
            if (genre == null)
            {
                _logger.LogWarning("Genre with Id {GenreId} not found!", id);
                return NotFound($"Genre with Id '{id}' not found!");
            }

            // name already reserved?
            var exist = await _genreRepository.AnyAsync(p => p.Name == genreDto.Name);
            if (exist)
            {
                _logger.LogWarning("Genre NAME '{GenreName}' already exist!", genreDto.Name);
                return Conflict($"Genre NAME '{genreDto.Name}' already exist!");
            }

            genre.Name = genreDto.Name;


            await _genreRepository.SaveChangesAsync();

            _logger.LogInformation("Genre '{GenreName}' successfully updated!", genre.Name);
            return Ok(new GenreDto { Id = genre.Id, Name = genre.Name });
        }
        catch (Exception)
        {
            _logger.LogError("Error occurred while trying to edit Genre in db!");
            return StatusCode(500, "Error occurred while trying edit Genre in db");
        }
    }



    // Get all films //
    [HttpGet("AllFilms")]
    public async Task<ActionResult<IEnumerable<FilmDto>>> GetAllFilms()
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while trying to retrieve films from the database.");
            return StatusCode(500, "Error occurred while trying to retrieve films from the database.");
        }
    }


    // ADD FILM //
    [HttpPost("AddFilm")]
    public async Task<ActionResult<FilmDto>> AddFilm(FilmDto film)
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

            var result = new FilmDto
            {
                Id = newFilm.Id,
                Director = newFilm.Director,
                ImageFileName = newFilm.ImageFileName,
                ReleaseDate = newFilm.ReleaseDate,
                Description = newFilm.Description,
                Name = newFilm.Name,
                Genres = film.Genres
            };

            return CreatedAtAction(nameof(AddFilm), new { id = result.Id }, result);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            _logger.LogError("Error occurred while trying to save Film '{FilmName}' to db!", film.Name);
            return StatusCode(500, "Error occurred while trying to save Film to db");
        }
    }


    // DELETE FILM //
    [HttpDelete("RemoveFilm/{id}")]
    public async Task<ActionResult> DeleteFilm(int id)
    {

        if (id <= 0)
        {
            _logger.LogWarning("Invalid film ID. ID must be greater than 0.");
            return BadRequest("Invalid film ID. ID must be greater than 0.");
        }


        using var transaction = await _filmRepository.BeginTransactionAsync();
        try
        {
            var film = await _filmRepository.GetList()
             .Include(f => f.FilmGenres)
             .Include(f => f.Ratings)
             .Include(f => f.Comments)
             .FirstOrDefaultAsync(f => f.Id == id);

            if (film == null)
            {
                _logger.LogWarning("Film with Id {FilmId} not exist!", id);
                return NotFound($"Film with Id '{id}' not exist!");
            }

            _filmRepository.Remove(film);
            await _filmRepository.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Film '{FilmName}' successfully deleted!", film.Name);
            return Ok();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            _logger.LogError("Error occurred while trying to save Film to db!");
            return StatusCode(500, "Error occurred while trying to save Film to db");
        }
    }

    // EDIT FILM //
    [HttpPut("EditFilm/{id}")]
    public async Task<ActionResult<FilmDto>> PutFilm(int id, [FromBody] FilmDto filmDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Film is not valid {ModelState}", ModelState);
            return BadRequest($"Film is not valid {ModelState}");
        }

        if (id <= 0)
        {
            _logger.LogWarning("Invalid film ID. ID must be greater than 0.");
            return BadRequest("Invalid film ID. ID must be greater than 0.");
        }

        // find film with id in db

        var film = await _filmRepository.GetList()
            .Include(f => f.FilmGenres)
            .Include(f => f.Ratings)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (film == null)
        {
            _logger.LogWarning("Film with Id {FilmId} not found!", id);
            return NotFound($"Film with Id '{id}' not found!");
        }

        using var transaction = await _filmRepository.BeginTransactionAsync();
        try
        {
            // name already reserved?
            var exist = await _genreRepository.GetList().AnyAsync(p => p.Name == filmDto.Name && p.Id != id);
            if (exist)
            {
                _logger.LogWarning("Genre NAME '{FilmName}' already exist!", filmDto.Name);
                return Conflict($"Genre NAME '{filmDto.Name}' already exist!");
            }

            film.Name = filmDto.Name;
            film.Director = filmDto.Director;
            film.Description = filmDto.Description;
            film.ImageFileName = filmDto.ImageFileName;
            film.ReleaseDate = filmDto.ReleaseDate;


            // old film-genres - remove
            _filmGenreRepository.RemoveRange(film.FilmGenres);

            // new film-genres - add
            var newFilmGenres = filmDto.Genres.Select(p => new FilmGenre { FilmId = film.Id, GenreId = p.Id });
            await _filmGenreRepository.AddRangeAsync(newFilmGenres);

            await _filmGenreRepository.SaveChangesAsync();
            await transaction.CommitAsync();
            _logger.LogInformation("Film '{FilmName}' successfully updated!", film.Name);


            var resultDto = new FilmDto
            {
                Id = film.Id,
                Name = film.Name,
                Description = film.Description,
                Director = film.Director,
                ImageFileName = film.ImageFileName,
                ReleaseDate = film.ReleaseDate,
                Genres = filmDto.Genres
            };

            return Ok(resultDto);

        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            _logger.LogError("Error occurred while trying to save Film '{FilmName}' to db!", film.Name);
            return StatusCode(500, "Error occurred while trying to save Film to db");
        }
    }





}
