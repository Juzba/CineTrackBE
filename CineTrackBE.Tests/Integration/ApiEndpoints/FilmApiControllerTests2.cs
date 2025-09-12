using CineTrackBE.AppServices;
using CineTrackBE.Models.DTO;
using CineTrackBE.Models.Entities;
using CineTrackBE.Tests.Helpers.Common;
using CineTrackBE.Tests.Helpers.TestSetups.Universal;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace CineTrackBE.Tests.Integration.ApiEndpoints;

public class FilmApiControllerTests2
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
        await HttpContextTestSetup.Create().BuildAndSaveAsync(setup.Controller, setup.Context);

        // Act 
        var result = await setup.Controller.GetFilm(1);

        // Assert
        result.Should().NotBeNull();
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        statusResult.Value.Should().Be("An error occurred while fetching film details.");

        mockFilmRepository.Verify(x => x.GetList(), Times.Once);
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
               .Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Film, bool>>>(), It.IsAny<CancellationToken>()))
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

        mockFilmRepository.Verify(x => x.AnyAsync(It.IsAny<Expression<Func<Film, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task ToggleFavorite__Should_Return_BadRequest_When_FilmId_IsWrongFormat()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();
        await HttpContextTestSetup.Create().BuildAndSaveAsync(setup.Controller, setup.Context);
        int wrongId = 0;

        // Act 
        var result = await setup.Controller.ToggleFavorite(wrongId);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        badRequestResult.Value.Should().Be("Film ID must be greater than 0.");
    }

    [Fact]
    public async Task ToggleFavorite__Should_Return_NotFound_When_FilmId_NotExist_InDb()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();
        await HttpContextTestSetup.Create().BuildAndSaveAsync(setup.Controller, setup.Context);
        int nonExistedId = 999;

        // Act 
        var result = await setup.Controller.ToggleFavorite(nonExistedId);

        // Assert
        result.Should().NotBeNull();
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        notFoundResult.Value.Should().Be("Film with ID 999 not found.");
    }

    [Fact]
    public async Task ToggleFavorite__Should_AddMovie_ToUserFavorite_AndReturn_OkResult_WithValueTrue()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();
        var (user, _, _) = await HttpContextTestSetup.Create().BuildAndSaveAsync(setup.Controller, setup.Context);

        var films = await Fakers.Film.GenerateAndSaveAsync(3, setup.Context);
        var targetedFilmId = films[1].Id;

        // Act 
        var result = await setup.Controller.ToggleFavorite(targetedFilmId);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var boolResult = okResult.Value.Should().BeOfType<bool>().Subject;
        boolResult.Should().BeTrue();
    }

    [Fact]
    public async Task ToggleFavorite__Should_RemoveMovie_FromUserFavorite_AndReturn_OkResult_WithValueFalse()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();
        var (user, _, _) = await HttpContextTestSetup.Create().BuildAndSaveAsync(setup.Controller, setup.Context);

        var films = await Fakers.Film.GenerateAndSaveAsync(3, setup.Context);
        var targetedFilmId = films[1].Id;

        user.FavoriteMovies.Add(targetedFilmId);
        setup.Context.Update(user);
        await setup.Context.SaveChangesAsync();

        // Act 
        var result = await setup.Controller.ToggleFavorite(targetedFilmId);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var boolResult = okResult.Value.Should().BeOfType<bool>().Subject;
        boolResult.Should().BeFalse();
    }


    [Fact]
    public async Task AddComment__Should_Return_OkResult_WithComment()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();
        await HttpContextTestSetup.Create().BuildAndSaveAsync(setup.Controller, setup.Context);

        var film = await Fakers.Film.GenerateOneAndSaveAsync(setup.Context);
        var commentWithRating = Fakers.CommentWithRatingDto.Generate();
        commentWithRating.FilmId = film.Id;

        // Act
        var result = await setup.Controller.AddComment(commentWithRating);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var commentWithRatingDtoResult = okResult.Value.Should().BeOfType<CommentWithRatingDto>().Subject;
        commentWithRatingDtoResult.Should()
            .BeEquivalentTo(commentWithRating, o => o
            .Including(p => p.Text)
            .Including(p => p.Rating)
            .Including(p => p.SendDate));

    }

    [Fact]
    public async Task AddComment__Should_Return_StatusCode500_When_ErrorOccurredInDb()
    {
        // Arrange
        var mockCommentRepository = new Mock<IRepository<Comment>>();
        mockCommentRepository
            .Setup(x => x.AddAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock
            .Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mockCommentRepository
            .Setup(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionMock.Object);

        using var setup = FilmApiControllerTestSetup.Create(commentRepository: mockCommentRepository.Object);
        await HttpContextTestSetup.Create().BuildAndSaveAsync(setup.Controller, setup.Context);

        var commentWithRating = Fakers.CommentWithRatingDto.Generate();

        // Act 
        var result = await setup.Controller.AddComment(commentWithRating);

        // Assert
        result.Should().NotBeNull();
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        statusResult.Value.Should().Be("An error occurred while adding the comment.");

        // Verify method calls
        mockCommentRepository.Verify(x => x.AddAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()), Times.Once);
        transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddComment__Should_Return_Unauthorized_When_UserAndUserClaims_AreWrong()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();

        var commentWithRating = Fakers.CommentWithRatingDto.Generate();

        // Act 
        var result = await setup.Controller.AddComment(commentWithRating);

        // Assert
        result.Should().NotBeNull();
        var statusResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        statusResult.Value.Should().Be("User not authenticated!");
    }

    [Fact]
    public async Task AddComment__Should_Return_StatusCode500_When_NewRatingThrowDbError_OnAddAsyncToDb()
    {
        // Arrange
        var ratingMock = new Mock<IRepository<Rating>>();
        ratingMock.Setup(o => o
            .AddAsync(It.IsAny<Rating>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error on AddAsync 'Rating'"));


        using var setup = FilmApiControllerTestSetup.Create(ratingRepository: ratingMock.Object);
        await HttpContextTestSetup.Create().BuildAndSaveAsync(setup.Controller, setup.Context);

        var film = await Fakers.Film.GenerateOneAndSaveAsync(setup.Context);
        var commentWithRating = Fakers.CommentWithRatingDto.Generate();
        commentWithRating.FilmId = film.Id;

        // Act 
        var result = await setup.Controller.AddComment(commentWithRating);

        // Assert
        result.Should().NotBeNull();
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        statusResult.Value.Should().Be("An error occurred while adding the comment.");

        var commentsInDb = await setup.Context.Comments.ToListAsync();
        commentsInDb.Should().BeEmpty();

        var ratingsInDb = await setup.Context.Ratings.ToListAsync();
        ratingsInDb.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Text", null, "The Text field is required.")]
    [InlineData("Text", "", "Minimal text lenght is 1!")]
    [InlineData("FilmId", 0, "FilmId is required!")]
    [InlineData("Rating", -1, "Rating musí být mezi 0 a 100!")]
    [InlineData("Rating", 101, "Rating musí být mezi 0 a 100!")]
    public async Task AddComment__Should_Return_BadRequest_When_ModelState_IsNotValid(string propertyName, object? invalidValue, string expectedError)
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();
        await HttpContextTestSetup.Create().BuildAndSaveAsync(setup.Controller, setup.Context);

        var commentWithRating = Fakers.CommentWithRatingDto.Generate();

        // Set invalid value using reflection
        typeof(CommentWithRatingDto).GetProperty(propertyName)!
            .SetValue(commentWithRating, invalidValue);

        setup.Controller.ModelState.AddModelError(propertyName, expectedError);

        // Act
        var result = await setup.Controller.AddComment(commentWithRating);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        var errorResponse = badRequestResult.Value.Should().BeOfType<SerializableError>().Subject;
        errorResponse.Should().ContainKey(propertyName);
    }












}
