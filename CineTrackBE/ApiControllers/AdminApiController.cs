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
public class AdminApiController(IRepository<IdentityUserRole<string>> userRoleRepository, ILogger<AdminApiController> logger, IRepository<Film> filmRepository, IRepository<Genre> genreRepository, IRepository<FilmGenre> filmGenreRepository, IRepository<ApplicationUser> userRepository, IRepository<Rating> ratingRepository, IRepository<Comment> commentRepository) : ControllerBase
{
    private readonly IRepository<Genre> _genreRepository = genreRepository;
    private readonly IRepository<FilmGenre> _filmGenreRepository = filmGenreRepository;
    private readonly IRepository<Film> _filmRepository = filmRepository;
    private readonly IRepository<ApplicationUser> _userRepository = userRepository;
    private readonly IRepository<IdentityUserRole<string>> _userRoleRepository = userRoleRepository;
    private readonly ILogger<AdminApiController> _logger = logger;
    private readonly IRepository<Rating> _ratingRepository = ratingRepository;
    private readonly IRepository<Comment> _commentRepository = commentRepository;


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

        using var transaction = await _filmRepository.BeginTransactionAsync();
        try
        {
            var exist = await _filmRepository.AnyAsync(p => p.Name == film.Name);
            if (exist)
            {
                _logger.LogInformation("Film already Exist {FilmName}", film.Name);
                return Conflict($"Film '{film.Name}' already Exist");
            }

            // if genre is not in db
            var existGenres = await _genreRepository.GetAllAsync();
            foreach (var genreDto in film.Genres)
            {
                if (!existGenres.Any(p => p.Id == genreDto.Id))
                {
                    _logger.LogWarning("Genre with Id {GenreId} does not exist in db!", genreDto.Id);
                    return BadRequest($"Genre with Id '{genreDto.Id}' does not exist in db!");
                }
            }

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

            _logger.LogInformation("Film '{FilmName}' successfully deleted!", film.Name);
            return Ok();
        }
        catch (Exception)
        {
            _logger.LogError("Error occurred while trying to save Film to db!");
            return StatusCode(500, "Error occurred while trying to save Film to db");
        }
    }

    // EDIT FILM //
    [HttpPut("EditFilm/{id}")]
    public async Task<ActionResult<FilmDto>> EditFilm(int id, [FromBody] FilmDto filmDto)
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

        using var transaction = await _filmRepository.BeginTransactionAsync();
        try
        {
            var film = await _filmRepository.GetList()
                .Include(f => f.FilmGenres)
                .Include(f => f.Ratings)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (film == null)
            {
                _logger.LogWarning("Film with Id {FilmId} not found!", id);
                return NotFound($"Film with Id '{id}' not found!");
            }
            // film name already reserved?
            var exist = await _filmRepository.GetList().AnyAsync(p => p.Name == filmDto.Name && p.Id != id);
            if (exist)
            {
                _logger.LogWarning("Film NAME '{FilmName}' already exist!", filmDto.Name);
                return Conflict($"Film NAME '{filmDto.Name}' already exist!");
            }

            film.Name = filmDto.Name;
            film.Director = filmDto.Director;
            film.Description = filmDto.Description;
            film.ImageFileName = filmDto.ImageFileName;
            film.ReleaseDate = filmDto.ReleaseDate;


            // old film-genres - remove
            if (film.FilmGenres.Count != 0)
            {
                _filmGenreRepository.RemoveRange(film.FilmGenres);
            }

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
            _logger.LogError("Error occurred while trying to save Film to db!");
            return StatusCode(500, "Error occurred while trying to save Film to db");
        }
    }

    // WEB STATISTICS //
    [HttpGet("Statistics")]
    public async Task<ActionResult<StatisticsDto>> GetWebStatistics()
    {
        try
        {
            var movies = _filmRepository.GetList().Include(p => p.Comments).Include(p => p.Ratings);
            var users = _userRepository.GetList().Include(p => p.Comments);
            var ratings = _ratingRepository.GetList();
            var comments = _commentRepository.GetList();

            var totalUsers = await users.CountAsync();
            var totalComments = await comments.CountAsync();
            var totalRating = await ratings.CountAsync();
            var averageRating = totalRating > 0 ? await ratings.Select(p => p.UserRating).AverageAsync() : 0;

            // best rated films
            var bestRatedFilms = await movies.Where(p=>p.Ratings.Any())
                .OrderByDescending(p => p.Ratings.Select(p => p.UserRating)
                .Average()).Take(3)
                .ToListAsync();

            // most popular
            var mostPopularFilms = await movies.OrderByDescending(p => p.Comments.Count()).Take(3).ToListAsync();
            // Latest films
            var latestFilms = await movies.OrderByDescending(p => p.ReleaseDate).Take(3).ToListAsync();
            // top active users
            var mostActiveUsers = await users.OrderByDescending(p => p.Comments.Count()).Take(3).ToListAsync();

            var overview = new Overview
            {
                TotalMovies = await movies.CountAsync(),
                AverageRating = averageRating,
                TotalRatings = totalRating,
                TotalComments = totalComments,
                TotalUsers = totalUsers
            };

            var topMovies = new TopMovies
            {
                BestRated = [.. bestRatedFilms.Select(p => new FilmDto { Id = p.Id, Name = p.Name, Director = p.Director, ReleaseDate = p.ReleaseDate, ImageFileName = p.ImageFileName })],
                MostPopular = [.. mostPopularFilms.Select(p => new FilmDto { Id = p.Id, Name = p.Name, Director = p.Director, ReleaseDate = p.ReleaseDate, ImageFileName = p.ImageFileName })],
                Newest = [.. latestFilms.Select(p => new FilmDto { Id = p.Id, Name = p.Name, Director = p.Director, ReleaseDate = p.ReleaseDate, ImageFileName = p.ImageFileName })]
            };

            var userActivity = new UserActivity
            {
                AverageCommentsPerUser = totalUsers > 0 ? (double)totalComments / totalUsers : 0,
                MostActiveUsers = [.. mostActiveUsers.Select(p => new UserDto { Id = p.Id, Email = p.Email, UserName = p.UserName })]
            };

            var statisticsDto = new StatisticsDto
            {
                Overview = overview,
                TopMovies = topMovies,
                UserActivity = userActivity
            };

            _logger.LogInformation("Web statistics successfully retrieved.");
            return Ok(statisticsDto);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while trying to retrieve statistics from the database.");
            return StatusCode(500, "Error occurred while trying to retrieve statistics from the database.");
        }

    }



}
