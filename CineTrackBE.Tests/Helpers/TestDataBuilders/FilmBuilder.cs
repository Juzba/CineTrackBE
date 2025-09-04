using CineTrackBE.Data;
using CineTrackBE.Models.Entities;

namespace CineTrackBE.Tests.Helpers.TestDataBuilders;

// FILM BUILDER
public class FilmBuilder
{
    private Film _film = new();

    public static FilmBuilder Create() => new FilmBuilder();

    public FilmBuilder WithId(int id)
    {
        _film.Id = id;
        return this;
    }

    public FilmBuilder WithName(string name)
    {
        _film.Name = name;
        return this;
    }

    public FilmBuilder WithDirector(string name)
    {
        _film.Director = name;
        return this;
    }

    public FilmBuilder WithYear(int year)
    {
        var dateTime = new DateTime(year, 1, 1);
        _film.ReleaseDate = dateTime;
        return this;
    }

    public FilmBuilder WithRandomData()
    {
        _film.Name = $"Test Film {Guid.NewGuid().ToString()[..8]}";
        _film.Director = $"Test Director {Guid.NewGuid().ToString()[..8]}";

        var randomYear = Random.Shared.Next(1990, 2024);
        var randomMonth = Random.Shared.Next(1, 13);
        var randomDay = Random.Shared.Next(1, 28);

        _film.ReleaseDate = new DateTime(randomYear, randomMonth, randomDay);
        return this;
    }

    public Film Build() => _film;

    public async Task<Film> BuildAndSaveAsync(ApplicationDbContext context)
    {
        context.Films.Add(_film);
        await context.SaveChangesAsync();
        return _film;
    }
}

// FILM LIST BUILDER //
public class FilmListBuilder()
{
    private readonly List<Film> _filmList = [];


    public static FilmListBuilder Create(int count)
    {
        var filmBuilder = new FilmListBuilder();

        for (int i = 0; i < count; i++)
        {
            filmBuilder._filmList.Add(new Film());
        }

        return filmBuilder;
    }

    public FilmListBuilder WithRandomData()
    {

        foreach (var film in _filmList)
        {
            var randomYear = Random.Shared.Next(1980, 2026);

            film.Name = $"Test Film {Guid.NewGuid().ToString()[..8]}";
            film.Director = $"Test Director {Guid.NewGuid().ToString()[..8]}";
            film.ReleaseDate = new DateTime(randomYear, 1, 1);
        }

        return this;
    }

    public FilmListBuilder IncludeGenre()
    {

        foreach (var film in _filmList)
        {
            var filmGenres = new FilmGenre { FilmId = film.Id, Genre = new Genre { Name = $"Test Genre {Guid.NewGuid().ToString()[..8]}" } };

            film.FilmGenres.Add(filmGenres);
        }

        return this;
    }

    public List<Film> Build() => _filmList;

    public async Task<List<Film>> BuildAndSaveAsync(ApplicationDbContext context)
    {
        context.Films.AddRange(_filmList);
        await context.SaveChangesAsync();
        return _filmList;
    }


}

