using Bogus;
using CineTrackBE.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace CineTrackBE.Tests.Helpers.Common;


public static class Fakers
{
    public static readonly Faker<Genre> Genre =
        new Faker<Genre>()
            //.RuleFor(g => g.Id, f => f.Random.Int(1, 3))
            .RuleFor(g => g.Name, f => $"Genre {f.Random.AlphaNumeric(6)}");


    public static readonly Faker<IdentityRole> Role =
        new Faker<IdentityRole>()
            .RuleFor(g => g.Name, f => $"Role {f.Random.AlphaNumeric(6)}");



    public static readonly Faker<Film> Film =
        new Faker<Film>()
            //.RuleFor(fm => fm.Id, 0)  /// pridano
            .RuleFor(fm => fm.Name, f => $"Film {f.Random.AlphaNumeric(8)}")
            .RuleFor(fm => fm.Director, f => f.Person.FullName)
            .RuleFor(fm => fm.ReleaseDate, f => f.Date.Between(new DateTime(1980, 1, 1), new DateTime(2025, 12, 31)));



    public static readonly Faker<Film> FilmIncGenre =
          new Faker<Film>()
            .RuleFor(fm => fm.Name, f => $"FilmInclGenre {f.Random.AlphaNumeric(8)}")
            .RuleFor(fm => fm.Director, f => f.Person.FullName)
            .RuleFor(fm => fm.ReleaseDate, f => f.Date.Between(new DateTime(1980, 1, 1), new DateTime(2025, 12, 31)))
            .FinishWith((f, fm) =>
            {

                var g = Fakers.Genre.Generate();
                fm.FilmGenres.Add(new FilmGenre { Film = fm, Genre = g });
            });
}
