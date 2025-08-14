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

        [HttpGet]
        [Route("GetAll")]
        public async Task<IEnumerable<string>> GetFilms()
        {
            return await _filmRepository.GetList().Select(p => p.Name).ToListAsync();
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
