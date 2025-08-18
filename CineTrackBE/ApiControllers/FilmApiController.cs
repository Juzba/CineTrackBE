using CineTrackBE.Models.DTO;
using CineTrackBE.Models.Entities;
using CineTrackBE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilmApiController(IRepository<Film> filmRepository, IRepository<Genre> genreRepository) : ControllerBase
    {
        private readonly IRepository<Film> _filmRepository = filmRepository;
        private readonly IRepository<Genre> _genreRepository = genreRepository;


        // Top 5 Latest Films //
        [HttpGet]
        [Route("LatestFilms")]
        public async Task<ActionResult<IEnumerable<FilmDto>>> GetLatestFilms()
        {
            var films = await _filmRepository.GetList().OrderByDescending(p => p.ReleaseDate).Take(5).ToListAsync();

            if(films == null || films.Count == 0) return NotFound();

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


        //// Get all Films //
        //[HttpGet]
        //[Route("AllFilms")]
        //public async Task<ActionResult<IEnumerable<FilmDto>>> GetAllFilms()
        //{
        //    var films = await _filmRepository.GetList().ToListAsync();

        //    if(films == null || films.Count == 0) return NotFound();

        //    var filmsDTO = films.Select(p => new FilmDto()
        //    {
        //        Id = p.Id,
        //        Name = p.Name,
        //        Director = p.Director,
        //        ImageFileName = p.ImageFileName,
        //        Description = p.Description,
        //        ReleaseDate = p.ReleaseDate,
        //        Genres = [.. p.FilmGenres.Select(g => g.Genre.Name)]
        //    });

        //    return Ok(filmsDTO);
        //}


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


        [HttpPost]
        [Route("CatalogSearch")]
        public async Task<ActionResult<IEnumerable<Film>>> CatalogPost([FromBody] string value)
        {
            var films = await _filmRepository.GetList().ToListAsync();

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



        // GET api/<FilmApiController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
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
}
