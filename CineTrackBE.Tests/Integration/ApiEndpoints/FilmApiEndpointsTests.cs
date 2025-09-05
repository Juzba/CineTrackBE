using CineTrackBE.AppServices;
using CineTrackBE.Models.DTO;
using CineTrackBE.Models.Entities;
using CineTrackBE.Tests.Helpers.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CineTrackBE.Tests.Integration.ApiEndpoints;

public class FilmApiEndpointsTests
{

    [Fact]
    public async Task GetLatestFilms__Should_Return_OkResult()
    {
        // Arrange 
        using var setup = FilmApiControllerTestSetup.Create();

        var films = await Fakers.Film.GenerateAndSaveAsync(1, setup.Context);

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

        var films = await Fakers.Film.GenerateAndSaveAsync(6, setup.Context);

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

        var films = await Fakers.FilmIncGenre.GenerateAndSaveAsync(3, setup.Context);

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

        var films = await Fakers.Film.GenerateAndSaveAsync(5, setup.Context);

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

    [Fact]
    public async Task GetLatestFilms__Should_Return_InternalServerError_When_Exception_Occurs()
    {
        // Arrange
        var mockFilmRepository = new Mock<IRepository<Film>>();

        mockFilmRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        using var setup = FilmApiControllerTestSetup.Create(filmRepository: mockFilmRepository.Object);

        // Act
        var result = await setup.Controller.GetLatestFilms();

        // Assert
        result.Should().NotBeNull();
        var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
    }




    [Fact]
    public async Task GetAllGenres__Should_Return_OkResult_When_AllWorks()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();
        var genres = await Fakers.Genre.GenerateAndSaveAsync(1, setup.Context);

        // Act
        var result = await setup.Controller.GetAllGenres();

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<GenreDto>>();
    }


    [Fact]
    public async Task GetAllGenres__Should_Return_GenreList_FromDb()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();
        var genres = await Fakers.Genre.GenerateAndSaveAsync(3, setup.Context);

        // Act
        var result = await setup.Controller.GetAllGenres();

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedGenres = okResult.Value.Should().BeAssignableTo<IEnumerable<GenreDto>>().Subject;

        returnedGenres.Should().NotBeEmpty();
        returnedGenres.Should().BeEquivalentTo(genres, options => options
            .Including(p => p.Name)
            .Including(p => p.Id));
    }


    [Fact]
    public async Task GetAllGenres__Should_Return_EmptyList_When_DbIsEmpty()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();

        // Act
        var result = await setup.Controller.GetAllGenres();

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedGenres = okResult.Value.Should().BeAssignableTo<IEnumerable<GenreDto>>().Subject;

        returnedGenres.Should().BeEmpty();
    }


    [Fact]
    public async Task GetAllGenres__Should_Return_InternalServerError_When_Exception_Occurs()
    {
        // Arrange
        var mockGenreRepository = new Mock<IRepository<Genre>>();

        mockGenreRepository
            .Setup(x => x.GetAllAsync(default))
            .ThrowsAsync(new Exception("Database error"));

        using var setup = FilmApiControllerTestSetup.Create(genreRepository: mockGenreRepository.Object);

        // Act
        var result = await setup.Controller.GetAllGenres();

        // Assert
        result.Should().NotBeNull();
        var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task CatalogPost__Should_Return_OkStatus_WithEnumerableList_When_AllIsOk()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();
        var films = await Fakers.FilmIncGenre.GenerateAndSaveAsync(3, setup.Context);

        // Act
        var result = await setup.Controller.CatalogPost(null);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<FilmDto>>();
    }

    [Fact]
    public async Task CatalogPost__Should_Return_InternalServerError_When_Exception_Occurs()
    {
        // Arrange
        var mockFilmRepository = new Mock<IRepository<Film>>();
        mockFilmRepository
            .Setup(x => x.GetList())
            .Throws(new Exception("Database error"));
        using var setup = FilmApiControllerTestSetup.Create(filmRepository: mockFilmRepository.Object);

        // Act
        var result = await setup.Controller.CatalogPost(null);

        // Assert
        result.Should().NotBeNull();
        var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task CatalogPost__Should_Return_ListIncludeGenres_When_AllIsOk()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();
        var films = await Fakers.FilmIncGenre.GenerateAndSaveAsync(3, setup.Context);

        var expectedGenreName = films.OrderByDescending(p => p.Id).FirstOrDefault()?.FilmGenres.FirstOrDefault()?.Genre.Name;
        expectedGenreName.Should().NotBeNullOrEmpty("Test setup should create films with genres");

        // Act
        var result = await setup.Controller.CatalogPost(null);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedFilms = okResult.Value.Should().BeAssignableTo<IEnumerable<FilmDto>>().Subject;
        /// Include genre
        returnedFilms.Should().OnlyContain(f => f.Genres != null && f.Genres.Any());
        returnedFilms.First().Genres.First().Name.Should().Be(expectedGenreName);
    }

    [Fact]
    public async Task CatalogPost__Should_ReturnFilms_WhereName_ContainsParametr()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();
        var films = await Fakers.Film.GenerateAndSaveAsync(5, setup.Context);

        var searchTerm = films[0].Name.Substring(3, 3);
        var expectedFilms = films.Where(f => f.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
        expectedFilms.Should().NotBeEmpty("Test setup should create films with names containing the search term");

        // Act
        var result = await setup.Controller.CatalogPost(new SearchParametrsDto { SearchText = searchTerm });

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedFilms = okResult.Value.Should().BeAssignableTo<IEnumerable<FilmDto>>().Subject;

        returnedFilms.Should().HaveCount(expectedFilms.Count);
        returnedFilms.Should().BeEquivalentTo(expectedFilms, options => options
            .Including(p => p.Name)
            .Including(p => p.Director)
            .Including(p => p.ReleaseDate));

    }


    [Fact]
    public async Task CatalogPost__Should_ReturnFilms_Where_ReleasedDate_IsSameAs_ContainsParametr()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();
        var films = await Fakers.Film.GenerateAndSaveAsync(5, setup.Context);

        var searchByYear = films[0].ReleaseDate.Year;
        var expectedFilms = films.Where(f => f.ReleaseDate.Year == searchByYear).ToList();
        expectedFilms.Should().NotBeEmpty("Test setup should create films with release dates is same as searchByYear Parametr");

        // Act
        var result = await setup.Controller.CatalogPost(new SearchParametrsDto { SearchByYear = searchByYear });

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedFilms = okResult.Value.Should().BeAssignableTo<IEnumerable<FilmDto>>().Subject;

        returnedFilms.Should().HaveCount(expectedFilms.Count);
        returnedFilms.Should().BeEquivalentTo(expectedFilms, options => options
            .Including(p => p.Name)
            .Including(p => p.Director)
            .Including(p => p.ReleaseDate));
    }


    [Fact]
    public async Task CatalogPost__Should_ReturnFilms_WhereFilms_Are_SearchByGenre()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();


        var genres = await Fakers.Genre.GenerateAndSaveAsync(3, setup.Context);
        var targetGenre = genres.First();

        var films = await Fakers.Film.GenerateAndSaveAsync(5, setup.Context);

        films[0].FilmGenres.Add(new FilmGenre { FilmId = films[0].Id, GenreId = targetGenre.Id });
        films[1].FilmGenres.Add(new FilmGenre { FilmId = films[1].Id, GenreId = targetGenre.Id });
        films[2].FilmGenres.Add(new FilmGenre { FilmId = films[2].Id, GenreId = genres[1].Id });
        films[3].FilmGenres.Add(new FilmGenre { FilmId = films[3].Id, GenreId = genres[2].Id });
        films[4].FilmGenres.Add(new FilmGenre { FilmId = films[4].Id, GenreId = genres[1].Id });

        await setup.Context.SaveChangesAsync();

        var expectedFilms = films.Where(p => p.FilmGenres.Any(p => p.GenreId == targetGenre.Id)).ToList();

        // Act
        var result = await setup.Controller.CatalogPost(new SearchParametrsDto { GenreId = targetGenre.Id });

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedFilms = okResult.Value.Should().BeAssignableTo<IEnumerable<FilmDto>>().Subject.ToList();

        returnedFilms.Should().HaveCount(expectedFilms.Count);
        returnedFilms.Should().BeEquivalentTo(expectedFilms, options => options
        .Including(p => p.Name)
        .Including(p => p.Director)
        .Including(p => p.ReleaseDate));

    }

    [Fact]
    public async Task CatalogPost__Should_ReturnFilms_OrderedBy_NameDescending()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();

        var films = await Fakers.Film.GenerateAndSaveAsync(5, setup.Context);
        var expectedFilms = films.OrderByDescending(p => p.Name).ToList();

        // Act
        var result = await setup.Controller.CatalogPost(new SearchParametrsDto { SearchOrder = "NameDesc" });

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedFilms = okResult.Value.Should().BeAssignableTo<IEnumerable<FilmDto>>().Subject.ToList();

        returnedFilms.Should().BeInDescendingOrder(f => f.Name);
        returnedFilms.Should().BeEquivalentTo(expectedFilms, options => options
                                                       .WithStrictOrdering()
                                                       .Including(p => p.Name)
                                                       .Including(p => p.Director)
                                                       .Including(p => p.ReleaseDate));

    }


    // NameAsc
    // YearDesc
    // YearAsc
    // no search param OrderByDescending(p => p.Id)
    // zkontrolovat jestli se vytvari nova instance fakera pro kazdy test





}