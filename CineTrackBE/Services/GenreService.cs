using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.Services;

public interface IGenreService
{
    IQueryable<Genre> GetGenreList();
    Task<Genre> AddGenreAsync(Genre genre, CancellationToken cancellationToken = default);
    void RemoveGenre(Genre genre);
    void UpdateGenre(Genre genre);
    Task<Genre?> GetGenreByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> AnyGenreExistsAsync(int id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class GenreService : IGenreService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GenreService> _logger;

    public GenreService(ApplicationDbContext context, ILogger<GenreService> logger)
    {
        _context = context;
        _logger = logger;
    }



    // GET GENRE //
    public IQueryable<Genre> GetGenreList() => _context.Genre.AsQueryable();


    // ADD GENRE //
    public async Task<Genre> AddGenreAsync(Genre genre, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(genre);

        await _context.AddAsync(genre, cancellationToken);
        _logger.LogInformation("Genre {GenreName} added", genre.Name);
        return genre;
    }


    // REMOVE GENRE //
    public void RemoveGenre(Genre genre)
    {
        ArgumentNullException.ThrowIfNull(genre);

        _context.Genre.Remove(genre);
        _logger.LogInformation("Genre {GenreName} removed", genre.Name);
    }


    // UPDATE GENRE //
    public void UpdateGenre(Genre genre)
    {
        ArgumentNullException.ThrowIfNull(genre);

        var local = _context.Genre.Local.FirstOrDefault(p => p.Id == genre.Id);

        if (local != null) _context.Entry(local).State = EntityState.Detached;

        _context.Update(genre);
        _logger.LogInformation("Genre {GenreName} updated", genre.Name);
    }


    // GET GENRE BY ID //
    public async Task<Genre?> GetGenreByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Genre.FindAsync([id], cancellationToken);
    }


    // ANY GENRE EXIST? //
    public async Task<bool> AnyGenreExistsAsync(int id, CancellationToken cancellationToken = default) => await _context.Genre.AnyAsync(e => e.Id == id, cancellationToken);


    // SAVE CHANGES //
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) => await _context.SaveChangesAsync(cancellationToken);
}