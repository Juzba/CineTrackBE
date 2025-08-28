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
public class AdminApiController(IRepository<Film> filmRepository, IRepository<Genre> genreRepository, IRepository<FilmGenre> filmGenreRepository) : ControllerBase
{
    private readonly IRepository<Genre> _genreRepository = genreRepository;
    private readonly IRepository<FilmGenre> _filmGenreRepository = filmGenreRepository;
    private readonly IRepository<Film> _filmRepository = filmRepository;


    // ADD GENRE //
    [HttpPost("AddGenre")]
    public async Task<ActionResult<bool>> AddGenre(GenreDto genre)
    {

        if (!ModelState.IsValid) return BadRequest(ModelState);
        // Is Genre in db?
        var exist = await _genreRepository.GetList().AnyAsync(p => p.Name == genre.Name);
        if (exist) return Conflict($"Genre '{genre.Name}' already Exist");


        var newGenre = new Genre { Name = genre.Name };

        await _genreRepository.AddAsync(newGenre);
        var success = await _genreRepository.SaveChangesAsync();
        if (!success) return StatusCode(500, "Error occurred while trying to save Genre to db");

        return Ok(success);
    }


    // DELETE GENRE //
    [HttpDelete("RemoveGenre/{id}")]
    public async Task<ActionResult<bool>> DeleteGenre(int id)
    {
        if (id <= 0) return BadRequest("Invalid genre ID. ID must be greater than 0.");

        var genre = await _genreRepository.GetAsync_Id(id);
        if (genre == null) return NotFound($"Genre with Id '{id}' not exist!");


        var exist = await _filmGenreRepository.GetList().AnyAsync(p => p.GenreId == id);
        if (exist) return Conflict($"Genre '' cannot be deleted because it is used!");


        _genreRepository.Remove(genre);
        var success = await _genreRepository.SaveChangesAsync();
        if (!success) return StatusCode(500, "Error occurred while trying to save Genre to db");

        return Ok(success);
    }


    // EDIT GENRE //
    [HttpPut("EditGenre/{id}")]
    public async Task<ActionResult> PutGenre(int id, [FromBody] GenreDto genreDto)
    {
        if (!ModelState.IsValid) return BadRequest($"Genre is not valid {ModelState}");
        if (id <= 0) return BadRequest($"Invalid genre ID. ID must be greater than 0.");

        // find genre with id in db
        var genre = await _genreRepository.GetAsync_Id(id);
        if (genre == null) return NotFound($"Genre with Id '{id}' not found!");

        // name already reserved?
        var exist = await _genreRepository.GetList().AnyAsync(p => p.Name == genreDto.Name);
        if (exist) return Conflict($"Genre NAME '{genreDto.Name}' already exist!");


        genre.Name = genreDto.Name;
        _genreRepository.Update(genre);

        var success = await _genreRepository.SaveChangesAsync();
        if (!success) return StatusCode(500, "Error occurred while trying to save Genre to db");

        return Ok(success);
    }



    // Get all films //
    [HttpGet("AllFilms")]
    public async Task<ActionResult<IEnumerable<FilmDto>>> GetAllFilms()
    {
        var films = await _filmRepository
            .GetList()
            .Include(p=>p.FilmGenres)
            .ThenInclude(p=>p.Genre)
            .ToListAsync();

        var newFilms = films.Select(f => new FilmDto
        {
            Id = f.Id,
            Name = f.Name,
            Description = f.Description,
            Director = f.Director,
            ImageFileName = f.ImageFileName,
            ReleaseDate = f.ReleaseDate,
            Genres = f.FilmGenres?.Select(fg => new GenreDto { Id = fg.Genre.Id, Name = fg.Genre.Name}).ToList() ?? []
        });

        return Ok(newFilms);
    }









}
