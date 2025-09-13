using Bogus;
using CineTrackBE.Models.DTO;
using CineTrackBE.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace CineTrackBE.Tests.Helpers.Common;


public static class Fakers
{
    // GENRE //
    public static readonly Faker<Genre> Genre =
        new Faker<Genre>()
            .UseSeed(1234)
            .RuleFor(g => g.Name, f => $"Genre {f.Random.AlphaNumeric(6)}");


    // GENRE-DTO //
    public static readonly Faker<GenreDto> GenreDto =
        new Faker<GenreDto>()
            .UseSeed(1234)
            .RuleFor(g => g.Name, f => $"Genre {f.Random.AlphaNumeric(6)}");

    // USER //
    public static readonly Faker<ApplicationUser> User =
        new Faker<ApplicationUser>()
            .UseSeed(1234)
            .RuleFor(g => g.UserName, f => f.Name.FullName())
            .RuleFor(g => g.Email, f => $"TestEmail {f.Random.AlphaNumeric(6)}")
            .RuleFor(g => g.NormalizedEmail, (f, u) => u.Email?.ToUpper())
            .RuleFor(g => g.PasswordHash, f => $"PassWord {f.Random.Hash(10)}")
            .RuleFor(g => g.PhoneNumber, f => f.Phone.PhoneNumber());

    // USER-DTO //
    public static readonly Faker<UserDto> UserDto =
        new Faker<UserDto>()
            .UseSeed(1234)
            .RuleFor(g => g.UserName, f => f.Name.FullName())
            .RuleFor(g => g.Email, f => $"TestEmail {f.Random.AlphaNumeric(6)}")
            .RuleFor(g => g.NewPassword, f => $"PassWord {f.Random.Hash(10)}")
            .RuleFor(g => g.PhoneNumber, f => f.Phone.PhoneNumber());

    // ROLE //
    public static readonly Faker<IdentityRole> Role =
        new Faker<IdentityRole>()
            .UseSeed(1234)
            .RuleFor(g => g.Name, f => $"Role {f.IndexFaker}");

    // FILM //
    public static readonly Faker<Film> Film =
    new Faker<Film>()
        .UseSeed(1234)
        .CustomInstantiator(f => new Film
        {
            Name = $"Film {f.Random.AlphaNumeric(8)}",
            Director = f.Person.FullName,
            ReleaseDate = f.Date.Between(new DateTime(1980, 1, 1), new DateTime(2025, 12, 31)),
            FilmGenres = new List<FilmGenre>()
        });

    // FILM //
    public static readonly Faker<FilmDto> FilmDto =
    new Faker<FilmDto>()
        .UseSeed(1234)
        .CustomInstantiator(f => new FilmDto
        {
            Name = $"FilmDto {f.Random.AlphaNumeric(8)}",
            Director = f.Person.FullName,
            ReleaseDate = f.Date.Between(new DateTime(1980, 1, 1), new DateTime(2025, 12, 31)),
            Genres = []
        });


    // FILM INCLUDE GENRE //
    public static readonly Faker<Film> FilmIncGenre =
          new Faker<Film>()
            .UseSeed(1234)
            .RuleFor(fm => fm.Name, f => $"FilmInclGenre {f.Random.AlphaNumeric(8)}")
            .RuleFor(fm => fm.Director, f => f.Person.FullName)
            .RuleFor(fm => fm.ReleaseDate, f => f.Date.Between(new DateTime(1980, 1, 1), new DateTime(2025, 12, 31)))
            .RuleFor(fm => fm.FilmGenres, f =>
            {
                var genre = Fakers.Genre.Generate();
                return new List<FilmGenre> { new FilmGenre { Genre = genre } };
            });


    // RATING //
    public static readonly Faker<Rating> Rating =
          new Faker<Rating>()
            .UseSeed(1234)
            .RuleFor(fm => fm.UserRating, f => Random.Shared.Next(0, 101));


    // COMMENT-WITH-RATING-DTO //
    public static readonly Faker<CommentWithRatingDto> CommentWithRatingDto =
          new Faker<CommentWithRatingDto>()
            .UseSeed(1234)
            .RuleFor(fm => fm.Text, f => $"Test CommentWithRatingDto {f.Random.AlphaNumeric(8)}")
            .RuleFor(fm => fm.Rating, f => f.Random.Number(101))
            .RuleFor(fm => fm.SendDate, f => f.Date.Between(new DateTime(1980, 1, 1), new DateTime(2026, 12, 31)));


    // COMMENT //
    public static readonly Faker<Comment> Comment =
          new Faker<Comment>()
            .UseSeed(1234)
            .RuleFor(fm => fm.Text, f => $"Test Comment {f.Random.AlphaNumeric(8)}")
            .RuleFor(fm => fm.SendDate, f => f.Date.Between(new DateTime(1980, 1, 1), new DateTime(2026, 12, 31)));




    // COMMENT-INCLUDE-RATING //
    public static readonly Faker<Comment> CommentInclRating =
          new Faker<Comment>()
            .UseSeed(1234)
            .RuleFor(fm => fm.Text, f => $"Test Comment {f.Random.AlphaNumeric(8)}")
            .RuleFor(fm => fm.SendDate, f => f.Date.Between(new DateTime(1980, 1, 1), new DateTime(2026, 12, 31)))
            .FinishWith((f, fm) =>
            {
                var rating = Fakers.Rating.Generate();
                fm.Rating = rating;
                fm.RatingId = rating.Id;
            });

}
