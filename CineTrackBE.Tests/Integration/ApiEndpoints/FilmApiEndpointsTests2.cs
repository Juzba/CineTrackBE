using CineTrackBE.AppServices;
using CineTrackBE.Models.DTO;
using CineTrackBE.Models.Entities;
using CineTrackBE.Tests.Helpers.Common;
using CineTrackBE.Tests.Helpers.TestSetups;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CineTrackBE.Tests.Integration.ApiEndpoints;

public class FilmApiEndpointsTests2
{

    [Fact]
    public async Task GetFilm__Should_Return_UnAuthorized_When_User_NotExist_InDb()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();
        HttpContextTestSetup.Create().WithUser(userName: "This TestUser is not saved in Db").Build(setup.Controller);

        // Act
        var result = await setup.Controller.GetFilm(1);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        badRequestResult.Value.Should().Be("User not authenticated!");
    }

    [Fact]
    public async Task GetFilm__Should_Return_UnAuthorized_UserAndUserClaims_AreWrong()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();

        // Act
        var result = await setup.Controller.GetFilm(1);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        badRequestResult.Value.Should().Be("User not authenticated!");
    }



    [Fact]
    public async Task GetFilm__Should_Return_OkResult_WithFilm()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();
        await HttpContextTestSetup.Create().BuildAndSaveAsync(setup.Controller, setup.Context);

        var films = await Fakers.Film.GenerateAndSaveAsync(3, setup.Context);
        var targetedFilm = films[1];

        // Act
        var result = await setup.Controller.GetFilm(targetedFilm.Id);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var filmDto = okResult.Value.Should().BeOfType<FilmDto>().Subject;
        filmDto.Id.Should().Be(targetedFilm.Id);
    }

    [Fact]
    public async Task GetFilm__Should_Return_StatusCode500_When_ErrorOccurredInDb()
    {
        // Arrange
        var mockFilmRepository = new Mock<IRepository<Film>>();
        mockFilmRepository
            .Setup(x => x.GetList())
            .Throws(new Exception("Database error"));

        using var setup = FilmApiControllerTestSetup.Create(filmRepository: mockFilmRepository.Object);
        await HttpContextTestSetup.Create()
            .WithUser(userId: "test-user-id", userName: "testuser@test.com")
            .BuildAndSaveAsync(setup.Controller, setup.Context);

        // Act 
        var result = await setup.Controller.GetFilm(1);

        // Assert
        result.Should().NotBeNull();
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        statusResult.Value.Should().Be("An error occurred while fetching film details.");
    }

    [Fact]
    public async Task GetFilm__Should_Return_BadRequest_When_FilmId_IsWrongFormat()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();
        await HttpContextTestSetup.Create().BuildAndSaveAsync(setup.Controller, setup.Context);
        int wrongId = 0;

        // Act 
        var result = await setup.Controller.GetFilm(wrongId);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        badRequestResult.Value.Should().Be("Film ID must be greater than 0.");
    }



    [Fact]
    public async Task GetFilm__Should_Return_NotFound_When_FilmId_NotExist_InDb()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();
        await HttpContextTestSetup.Create().BuildAndSaveAsync(setup.Controller, setup.Context);
        int nonExistedId = 999;

        // Act 
        var result = await setup.Controller.GetFilm(nonExistedId);

        // Assert
        result.Should().NotBeNull();
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        notFoundResult.Value.Should().Be("Film with ID 999 not found.");
    }


    [Fact]
    public async Task GetFilm__Should_Return_NotFound_When_FilmDoesNotExist()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();
        await HttpContextTestSetup.Create().BuildAndSaveAsync(setup.Controller, setup.Context);

        int nonExistedFilmId = 999;

        // Act
        var result = await setup.Controller.GetFilm(nonExistedFilmId);

        // Assert
        result.Should().NotBeNull();
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        notFoundResult.Value.Should().Be($"Film with ID {nonExistedFilmId} not found.");
    }


    [Fact]
    public async Task GetFilm__Should_Return_OkResult_WithFavoriteFilm()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();
        var (user, _, _) = await HttpContextTestSetup.Create().BuildAndSaveAsync(setup.Controller, setup.Context);

        var film = await Fakers.Film.GenerateOneAndSaveAsync(setup.Context);
        var targetedFilmId = film.Id;

        user.FavoriteMovies.Add(targetedFilmId);
        setup.UserRepository.Update(user);
        await setup.UserRepository.SaveChangesAsync();

        // Act
        var result = await setup.Controller.GetFilm(targetedFilmId);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedFilm = okResult.Value.Should().BeOfType<FilmDto>().Subject;
        returnedFilm.Id.Should().Be(targetedFilmId);
        returnedFilm.IsMyFavorite.Should().BeTrue();
    }

    [Fact]
    public async Task GetFilm__Should_Return_OkResult_WithFilm_AverageRating()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();
        var (user, _, _) = await HttpContextTestSetup.Create().BuildAndSaveAsync(setup.Controller, setup.Context);

        var film = await Fakers.Film.GenerateOneAndSaveAsync(setup.Context);
        var targetedFilmId = film.Id;

        var comments = await Fakers.CommentInclRating
            .RuleFor(c => c.FilmId, _ => targetedFilmId)
            .GenerateAndSaveAsync(5, setup.Context);

        var expectedAverageRating = comments.Select(p => p.Rating.UserRating).Average();

        // Act
        var result = await setup.Controller.GetFilm(targetedFilmId);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedFilm = okResult.Value.Should().BeOfType<FilmDto>().Subject;
        returnedFilm.Id.Should().Be(targetedFilmId);
        returnedFilm.AvgRating.Should().Be(expectedAverageRating);
    }


    // ADD OR REMOVE FILM FROM FAVORITES //

    [Fact]
    public async Task ToggleFavorite__Should_Return_UnAuthorized_When_User_NotExist_InDb()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();
        HttpContextTestSetup.Create().WithUser(userName: "This TestUser is not saved in Db").Build(setup.Controller);

        // Act
        var result = await setup.Controller.ToggleFavorite(1);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        badRequestResult.Value.Should().Be("User not authenticated!");
    }

    [Fact]
    public async Task ToggleFavorite__Should_Return_UnAuthorized_UserAndUserClaims_AreWrong()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();

        // Act
        var result = await setup.Controller.ToggleFavorite(1);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        badRequestResult.Value.Should().Be("User not authenticated!");
    }

    [Fact]
    public async Task ToggleFavorite__Should_Return_OkResult_WithBool_True()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();
        await HttpContextTestSetup.Create().BuildAndSaveAsync(setup.Controller, setup.Context);

        var films = await Fakers.Film.GenerateAndSaveAsync(3, setup.Context);
        var targetedFilm = films[1];

        // Act
        var result = await setup.Controller.ToggleFavorite(targetedFilm.Id);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var boolResult = okResult.Value.Should().BeOfType<bool>().Subject;
        boolResult.Should().BeTrue();
    }

    [Fact]
    public async Task ToggleFavorite__Should_Return_StatusCode500_When_ErrorOccurredInDb()
    {
        // Arrange
        var mockFilmRepository = new Mock<IRepository<Film>>();
        mockFilmRepository
               .Setup(x => x.AnyExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new Exception("Database error"));

        using var setup = FilmApiControllerTestSetup.Create(filmRepository: mockFilmRepository.Object);
        await HttpContextTestSetup.Create().BuildAndSaveAsync(setup.Controller, setup.Context);

        // Act 
        var result = await setup.Controller.ToggleFavorite(1);

        // Assert
        result.Should().NotBeNull();
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        statusResult.Value.Should().Be("An error occurred while updating favorite films.");
    }


    // bad request when db throw error

    // wrong film id

    // film with id not exist in db

    //












}
