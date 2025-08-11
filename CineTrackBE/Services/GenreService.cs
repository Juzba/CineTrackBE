using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.Services;

public interface IGenreService
{

    Task<IEnumerable<Genre>> GetGenreList();
    Task AddGenre(Genre genre);
    void RemoveGenre(Genre genre);

    void UpdateGenre(Genre genre);
    Task<Genre?> GetGenre_Id(int id);
    bool AnyGenre_Id(int id);
    Task SaveChangesAsync();
}





public class GenreService(ApplicationDbContext context) : IGenreService
{

    private readonly ApplicationDbContext _context = context;




    // GET GENRE LIST //
    public async Task<IEnumerable<Genre>> GetGenreList() => await _context.Genre.ToListAsync();


    // ADD GENRE //
    public async Task AddGenre(Genre genre)
    {
        ArgumentNullException.ThrowIfNull(genre);

        await _context.AddAsync(genre);
    }


    // REMOVE GENRE //
    public void RemoveGenre(Genre genre)
    {
        ArgumentNullException.ThrowIfNull(genre);

        _context.Genre.Remove(genre);
    }

    // UPDATE GENRE //
    public void UpdateGenre(Genre genre)
    {
        ArgumentNullException.ThrowIfNull(genre);

        var local = _context.Genre.Local.FirstOrDefault(p => p.Id == genre.Id);

        if (local != null) _context.Entry(local).State = EntityState.Detached;

        _context.Update(genre);
    }


    // GET GENRE BY ID //
    public async Task<Genre?> GetGenre_Id(int id) => await _context.Genre.FindAsync(id);


    // ANY GENRE EXIST //
    public bool AnyGenre_Id(int id) => _context.Genre.Any(e => e.Id == id);


    // SAVE CHANGES //
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();





}

