using CineTrackBE.AppServices;
using CineTrackBE.Models.DTO;
using CineTrackBE.Models.Entities;
using CineTrackBE.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.ApiControllers;

[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]

public class FilmApiController(IRepository<Film> filmRepository, IRepository<ApplicationUser> userRepository, IRepository<Genre> genreRepository, IDataService dataService) : ControllerBase
{
    private readonly IRepository<Film> _filmRepository = filmRepository;
    private readonly IRepository<Genre> _genreRepository = genreRepository;
    private readonly IDataService _dataService = dataService;
    private readonly IRepository<ApplicationUser> _userRepository = userRepository;


    // Top 5 Latest Films //
    [HttpGet]
    [Route("LatestFilms")]
    public async Task<ActionResult<IEnumerable<FilmDto>>> GetLatestFilms()
    {
        var films = await _filmRepository.GetList().OrderByDescending(p => p.ReleaseDate).Take(5).ToListAsync();

        if (films == null || films.Count == 0) return NotFound();

        var filmsDTO = films.Select(p => new FilmDto()
        {
            Id = p.Id,
            Name = p.Name,
            Director = p.Director,
            ImageFileName = p.ImageFileName,
            Description = p.Description,
            ReleaseDate = p.ReleaseDate,
            Genres = [.. p.FilmGenres.Select(g => g.Genre.Name)]
        });

        return Ok(filmsDTO);
    }


    // Get all genres //
    [HttpGet]
    [Route("AllGenres")]
    public async Task<ActionResult<IEnumerable<GenreDto>>> GetAllGenres()
    {
        var genres = await _genreRepository.GetList().ToListAsync();

        if (genres == null || genres.Count == 0) return NotFound();

        var genresDto = genres.Select(p => new GenreDto()
        {
            Id = p.Id,
            Name = p.Name
        });

        return Ok(genresDto);
    }

    // SEARCH FILMS BY PARAMETERS //
    [HttpPost]
    [Route("CatalogSearch")]
    public async Task<ActionResult<IEnumerable<Film>>> CatalogPost([FromBody] SearchParametrsDto? searchParams)
    {
        // get film list from db
        var films = _dataService.GetFilmListAsync_InclFilmGenres();

        // search by name
        if (!string.IsNullOrWhiteSpace(searchParams?.SearchText))
        {
            films = films.Where(p => p.Name.Contains(searchParams.SearchText));
        }

        // search by release date
        if (searchParams?.SearchByYear != null && searchParams.SearchByYear > 0)
        {
            films = films.Where(p => p.ReleaseDate.Year == searchParams.SearchByYear);
        }

        // search by genre
        if (searchParams?.GenreId != null && searchParams.GenreId > 0)
        {
            films = films.Where(p => p.FilmGenres.Any(p => p.GenreId == searchParams.GenreId));
        }

        // order
        switch (searchParams?.SearchOrder)
        {
            case ("NameDesc"):
                films = films.OrderByDescending(p => p.Name);
                break;
            case ("NameAsc"):
                films = films.OrderBy(p => p.Name);
                break;
            case ("YearDesc"):
                films = films.OrderByDescending(p => p.ReleaseDate);
                break;
            case ("YearAsc"):
                films = films.OrderBy(p => p.ReleaseDate);
                break;
            default:
                films = films.OrderByDescending(p => p.Id);
                break;
        }

        var searchResult = await films.ToListAsync();

        // data transfer object
        var filmsDTO = searchResult.Select(p => new FilmDto()
        {
            Id = p.Id,
            Name = p.Name,
            Director = p.Director,
            ImageFileName = p.ImageFileName,
            Description = p.Description,
            ReleaseDate = p.ReleaseDate,
            Genres = [.. p.FilmGenres.Select(g => g.Genre.Name)]
        });

        return Ok(filmsDTO);
    }


    // GET FILM DETAILS BY ID //
    [HttpGet]
    [Route("FilmDetails/{id}")]
    public async Task<ActionResult<FilmDto>> GetFilm(int id)
    {
        if (id <= 0) return BadRequest("Film ID must be greater than 0.");

        var result = await _filmRepository.GetList().Include(p => p.FilmGenres).ThenInclude(p => p.Genre).FirstOrDefaultAsync(p => p.Id == id);

        if (result == null) return NotFound($"Film with ID {id} not found.");


        var filmsDTO = new FilmDto()
        {
            Id = result.Id,
            Name = result.Name,
            Director = result.Director,
            ImageFileName = result.ImageFileName,
            Description = result.Description,
            ReleaseDate = result.ReleaseDate,
            Genres = [.. result.FilmGenres.Select(g => g.Genre.Name)]
        };

        return Ok(filmsDTO);
    }


    //// ADD OR REMOVE FILM FROM FAVORITES //
    //[HttpGet]
    //[Route("ToggleFavorite/{filmId}")]
    //public async Task<ActionResult<bool>> ToggleFavorite(int filmId)
    //{
    //    if (filmId <= 0) return BadRequest("Film ID must be greater than 0.");

    //    // ziskat id z tokenu
    //    if (User?.Identity?.IsAuthenticated != true) return Unauthorized("User is not authenticated or does not exist.");
            

         




    //    var user = await _userRepository.GetAsync_Id(User.id)

    //    if (user == null) return Unauthorized("User not found.");

    //    if (user.FavoriteMovies.Any(p => p == filmId))
    //    {
    //        _userManager.
    //        //await _userManager.Users.FirstOrDefaultAsync(p => p.Id == user.Id)

                
    //    }



    //    //var result = await _dataService.ToggleFavoriteFilmAsync(filmId);


    //    if (result == null) return NotFound($"Film with ID {filmId} not found.");
    //    return Ok(result);
    }





    // PUT api/<FilmApiController>/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE api/<FilmApiController>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}
