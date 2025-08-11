using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.Services
{
    public interface IFilmService
    {

        Task<IEnumerable<Film>> GetFilmList();
        Task AddFilm(Film film);
        void RemoveFilm(Film film);

        void UpdateFilm(Film film);
        Task<Film?> GetFilm_Id(int id);
        bool AnyFilm_Id(int id);
        Task SaveChangesAsync();
    }





    public class FilmService(ApplicationDbContext context) : IFilmService
    {

        private readonly ApplicationDbContext _context = context;




        // GET FILM LIST //
        public async Task<IEnumerable<Film>> GetFilmList() => await _context.Films.ToListAsync();


        // ADD FILM //
        public async Task AddFilm(Film film)
        {
            ArgumentNullException.ThrowIfNull(film);

            await _context.AddAsync(film);
        }


        // REMOVE FILM //
        public void RemoveFilm(Film film)
        {
            ArgumentNullException.ThrowIfNull(film);

            _context.Films.Remove(film);
        }

        // UPDATE FILM //
        public void UpdateFilm(Film film)
        {
            ArgumentNullException.ThrowIfNull(film);

            var local = _context.Films.Local.FirstOrDefault(p => p.Id == film.Id);

            if (local != null) _context.Entry(local).State = EntityState.Detached;

            _context.Update(film);
        }


        // GET FILM BY ID //
        public async Task<Film?> GetFilm_Id(int id) => await _context.Films.FindAsync(id);


        // ANY FILM EXIST //
        public bool AnyFilm_Id(int id) => _context.Films.Any(e => e.Id == id);


        // SAVE CHANGES //
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();





    }
}
