using CineTrackBE.Data;
using CineTrackBE.Models.Entities;

namespace CineTrackBE.Tests.Helpers.TestDataBuilders;

// GENRE BUILDER
public class GenreBuilder
{
    private Genre _genre = new();

    public static GenreBuilder Create() => new();

    public GenreBuilder WithId(int id)
    {
        _genre.Id = id;
        return this;
    }

    public GenreBuilder WithName(string name)
    {
        _genre.Name = name;
        return this;
    }


    public GenreBuilder WithRandomData()
    {
        _genre.Name = $"Test Genre {Guid.NewGuid().ToString()[..8]}";
        return this;
    }

    public Genre Build() => _genre;

    public async Task<Genre> BuildAndSaveAsync(ApplicationDbContext context)
    {
        context.Genre.Add(_genre);
        await context.SaveChangesAsync();
        return _genre;
    }
}

// GENRE LIST BUILDER //
public class GenreListBuilder()
{
    private readonly List<Genre> _genreList = [];


    public static GenreListBuilder Create(int count)
    {
        var filmBuilder = new FilmListBuilder();

        for (int i = 0; i < count; i++)
        {
            filmBuilder._filmList.Add(new Genre());
        }

        return filmBuilder;
    }

    public GenreListBuilder WithRandomData()
    {

        foreach (var film in _genreList)
        {
            var randomYear = Random.Shared.Next(1980, 2026);

            film.Name = $"Test Film {Guid.NewGuid().ToString()[..8]}";
            film.Director = $"Test Director {Guid.NewGuid().ToString()[..8]}";
            film.ReleaseDate = new DateTime(randomYear, 1, 1);
        }

        return this;
    }

    public GenreListBuilder IncludeGenre()
    {

        foreach (var film in _genreList)
        {
            var filmGenres = new FilmGenre { FilmId = film.Id, Genre = new Genre { Name = $"Test Genre {Guid.NewGuid().ToString()[..8]}" } };

            film.FilmGenres.Add(filmGenres);
        }

        return this;
    }

    public List<Genre> Build() => _genreList;

    public async Task<List<Genre>> BuildAndSaveAsync(ApplicationDbContext context)
    {
        context.Films.AddRange(_genreList);
        await context.SaveChangesAsync();
        return _genreList;
    }


}
