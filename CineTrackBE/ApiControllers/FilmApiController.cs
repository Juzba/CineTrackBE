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


        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            return await _filmRepository.GetList().Select(p => p.Name).ToListAsync();
        }


        // Top 5 Latest Films //
        [HttpGet]
        [Route("LatestFilms")]
        public async Task<IEnumerable<FilmDTO>> GetTest()
        {
            var films = await _filmRepository.GetList().OrderBy(p=>p.ReleaseDate).Take(5).ToListAsync();

            var filmsDTO = films.Select(p => new FilmDTO()
            {
                Id = p.Id,
                Name = p.Name,
                Director = p.Director,
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
