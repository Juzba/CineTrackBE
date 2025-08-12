using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.Services
{
    public interface IFilmService
    {
        IQueryable<Film> GetFilmList();
        Task AddFilmAsync(Film film, CancellationToken cancellationToken = default);
        void RemoveFilm(Film film);
        void UpdateFilmAsync(Film film);
        Task<Film?> GetFilmByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AnyFilmExistsAsync(int id, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    public class FilmService(ApplicationDbContext context) : IFilmService
    {
        private readonly ApplicationDbContext _context = context;



        // GET FILM LIST //
        public IQueryable<Film> GetFilmList()
        {
            return _context.Films.AsQueryable();
        }


        // ADD FILM //
        public async Task AddFilmAsync(Film film, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(film);

            await _context.AddAsync(film, cancellationToken);
        }


        // REMOVE FILM //
        public void RemoveFilm(Film film)
        {
            ArgumentNullException.ThrowIfNull(film);

            _context.Films.Remove(film);
        }


        // UPDATE FILM //
        public void UpdateFilmAsync(Film film)
        {
            ArgumentNullException.ThrowIfNull(film);

            var local = _context.Films.Local.FirstOrDefault(p => p.Id == film.Id);

            if (local != null) _context.Entry(local).State = EntityState.Detached;

            _context.Update(film);
        }


        // GET FILM BY ID //
        public async Task<Film?> GetFilmByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Films.FindAsync([id], cancellationToken);
        }


        // ANY FILM EXIST //
        public async Task<bool> AnyFilmExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Films.AnyAsync(e => e.Id == id, cancellationToken);
        }


        // SAVE CHANGES //
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}