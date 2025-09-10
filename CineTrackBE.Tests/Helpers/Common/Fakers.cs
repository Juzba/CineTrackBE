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
            .RuleFor(g => g.Name, f => $"Genre {f.Random.AlphaNumeric(6)}");


    // GENRE-DTO //
    public static readonly Faker<GenreDto> GenreDto =
        new Faker<GenreDto>()
            .RuleFor(g => g.Name, f => $"Genre {f.Random.AlphaNumeric(6)}");

    // USER //
    public static readonly Faker<ApplicationUser> User =
        new Faker<ApplicationUser>()
            .RuleFor(g => g.UserName, f => $"UserName {f.Random.AlphaNumeric(6)}")
            .RuleFor(g => g.Email, f => $"TestEmail {f.Random.AlphaNumeric(6)}")
            .RuleFor(g => g.PasswordHash, f => $"PassWord {f.Random.Hash(10)}")
            .RuleFor(g => g.PhoneNumber, f => $"Test-PhoneNumber {f.Random.AlphaNumeric(6)}");
        

    // ROLE //
    public static readonly Faker<IdentityRole> Role =
        new Faker<IdentityRole>()
            .RuleFor(g => g.Name, f => $"Role {f.Random.AlphaNumeric(6)}");

    // FILM //
    public static readonly Faker<Film> Film =
        new Faker<Film>()
            .RuleFor(fm => fm.Name, f => $"Film {f.Random.AlphaNumeric(8)}")
            .RuleFor(fm => fm.Director, f => f.Person.FullName)
            .RuleFor(fm => fm.ReleaseDate, f => f.Date.Between(new DateTime(1980, 1, 1), new DateTime(2025, 12, 31)));


    // FILM INCLUDE GENRE //
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


    // RATING //
    public static readonly Faker<Rating> Rating =
          new Faker<Rating>()
            .RuleFor(fm => fm.UserRating, f => Random.Shared.Next(0, 101));


    // COMMENT-WITH-RATING-DTO //
    public static readonly Faker<CommentWithRatingDto> CommentWithRatingDto =
          new Faker<CommentWithRatingDto>()
            .RuleFor(fm => fm.Text, f => $"Test CommentWithRatingDto {f.Random.AlphaNumeric(8)}")
            .RuleFor(fm => fm.Rating, f => f.Random.Number(101))
            .RuleFor(fm => fm.SendDate, f => f.Date.Between(new DateTime(1980, 1, 1), new DateTime(2026, 12, 31)));


    // COMMENT //
    public static readonly Faker<Comment> Comment =
          new Faker<Comment>()
            .RuleFor(fm => fm.Text, f => $"Test Comment {f.Random.AlphaNumeric(8)}")
            .RuleFor(fm => fm.SendDate, f => f.Date.Between(new DateTime(1980, 1, 1), new DateTime(2026, 12, 31)));


    // COMMENT-INCLUDE-RATING //
    public static readonly Faker<Comment> CommentInclRating =
          new Faker<Comment>()
            .RuleFor(fm => fm.Text, f => $"Test Comment {f.Random.AlphaNumeric(8)}")
            .RuleFor(fm => fm.SendDate, f => f.Date.Between(new DateTime(1980, 1, 1), new DateTime(2026, 12, 31)))
            .FinishWith((f, fm) =>
            {
                var rating = Fakers.Rating.Generate();
                fm.Rating = rating;
                fm.RatingId = rating.Id;
            });

}
