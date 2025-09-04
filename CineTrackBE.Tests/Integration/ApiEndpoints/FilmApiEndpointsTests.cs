using CineTrackBE.Models.DTO;
using CineTrackBE.Tests.Helpers.TestDataBuilders;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CineTrackBE.Tests.Integration.ApiEndpoints;

public class FilmApiEndpointsTests
{

    [Fact]
    public async Task GetLatestFilms__Should_Return_OkResult()
    {
        // Arrange 
        using var setup = FilmApiControllerTestSetup.Create();

        var films = await FilmListBuilder
            .Create(1)
            .WithRandomData()
            .IncludeGenre()
            .BuildAndSaveAsync(setup.Context);

        // Act
        var result = await setup.Controller.GetLatestFilms();

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetLatestFilms__Should_Return_EmptyList()
    {
        // Arrange 
        using var setup = FilmApiControllerTestSetup.Create();

        // Act
        var result = await setup.Controller.GetLatestFilms();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedFilms = okResult.Value.Should().BeAssignableTo<IEnumerable<FilmDto>>().Subject;
        returnedFilms.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLatestFilms__Should_Return_Exactly_5_Films()
    {
        // Arrange 
        using var setup = FilmApiControllerTestSetup.Create();

        var films = await FilmListBuilder
            .Create(6)
            .WithRandomData()
            .IncludeGenre()
            .BuildAndSaveAsync(setup.Context);

        // Act
        var result = await setup.Controller.GetLatestFilms();

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedFilms = okResult.Value.Should().BeAssignableTo<IEnumerable<FilmDto>>().Subject;

        returnedFilms.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetLatestFilms__Should_Include_Genres()
    {
        // Arrange 
        using var setup = FilmApiControllerTestSetup.Create();

        var films = await FilmListBuilder
            .Create(6)
            .WithRandomData()
            .IncludeGenre()
            .BuildAndSaveAsync(setup.Context);

        var expectedGenreName = films.OrderByDescending(p => p.ReleaseDate).FirstOrDefault()?.FilmGenres.FirstOrDefault()?.Genre.Name;
        expectedGenreName.Should().NotBeNullOrEmpty("Test setup should create films with genres");

        // Act
        var result = await setup.Controller.GetLatestFilms();

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedFilms = okResult.Value.Should().BeAssignableTo<IEnumerable<FilmDto>>().Subject;

        /// Include genre
        returnedFilms.Should().OnlyContain(f => f.Genres != null && f.Genres.Any());
        returnedFilms.First().Genres.First().Name.Should().Be(expectedGenreName);
    }

    [Fact]
    public async Task GetLatestFilms__Should_Return_Films_Ordered_By_ReleaseDate_Desc()
    {
        // Arrange 
        using var setup = FilmApiControllerTestSetup.Create();

        var films = await FilmListBuilder
            .Create(6)
            .WithRandomData()
            .IncludeGenre()
            .BuildAndSaveAsync(setup.Context);

        var expectedFilms = films.OrderByDescending(p => p.ReleaseDate).Take(5).ToList();

        // Act
        var result = await setup.Controller.GetLatestFilms();

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedFilms = okResult.Value.Should().BeAssignableTo<IEnumerable<FilmDto>>().Subject;

        returnedFilms.Should().BeEquivalentTo(expectedFilms, options => options
        .Including(p => p.Name)
        .Including(p => p.Director)
        .Including(p => p.ReleaseDate));
    }

    //[Fact]
    //public async Task GetAllGenres__Should_Return_OkResult()
    //{
    //    // Arrange
    //    using var setup = FilmApiControllerTestSetup.Create();
    //    var genres = 

    //    // Act

    //    // Assert
    //}







}
