using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.Services
{
    public interface IFilmService
    {
        Task<IEnumerable<Film>> GetFilmList();
        Task<Film?> GetFilm_Id(int id);
    }





    public class FilmService(ApplicationDbContext context) : IFilmService
    {

        private readonly ApplicationDbContext _context = context;




        // GET FILM LIST //
        public async Task<IEnumerable<Film>> GetFilmList() => await _context.Films.ToListAsync();

        // GET FILM BY ID //
        public async Task<Film?> GetFilm_Id(int id) => await _context.Films.FindAsync(id);









    }
}
