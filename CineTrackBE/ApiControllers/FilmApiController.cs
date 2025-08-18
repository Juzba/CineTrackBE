using CineTrackBE.Models.DTO;
using CineTrackBE.Models.Entities;
using CineTrackBE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilmApiController(IRepository<Film> filmRepository) : ControllerBase
    {
        private readonly IRepository<Film> _filmRepository = filmRepository;


        // Top 5 Latest Films //
        [HttpGet]
        [Route("LatestFilms")]
        public async Task<IEnumerable<FilmDTO>> GetLatestFilms()
        {
            var films = await _filmRepository.GetList().OrderByDescending(p=>p.ReleaseDate).Take(5).ToListAsync();

            var filmsDTO = films.Select(p => new FilmDTO()
            {
                Id = p.Id,
                Name = p.Name,
                Director = p.Director,
                ImageFileName = p.ImageFileName,
                Description = p.Description,
                ReleaseDate = p.ReleaseDate,
                Genres = [.. p.FilmGenres.Select(g => g.Genre.Name)]
            });

            return filmsDTO;
        }


        // Get all Films //
        [HttpGet]
        [Route("AllFilms")]
        public async Task<IEnumerable<FilmDTO>> GetTest()
        {
            var films = await _filmRepository.GetList().ToListAsync();

            var filmsDTO = films.Select(p => new FilmDTO()
            {
                Id = p.Id,
                Name = p.Name,
                Director = p.Director,
                ImageFileName = p.ImageFileName,
                Description = p.Description,
                ReleaseDate = p.ReleaseDate,
                Genres = [.. p.FilmGenres.Select(g => g.Genre.Name)]
            });

            return filmsDTO;
        }




        // GET api/<FilmApiController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<FilmApiController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
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
