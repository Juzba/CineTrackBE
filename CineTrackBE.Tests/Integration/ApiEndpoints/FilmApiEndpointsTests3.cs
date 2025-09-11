using CineTrackBE.AppServices;
using CineTrackBE.Models.DTO;
using CineTrackBE.Models.Entities;
using CineTrackBE.Tests.Helpers.Common;
using CineTrackBE.Tests.Helpers.TestSetups;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NuGet.Packaging;
using System.Linq.Expressions;
using Xunit;

namespace CineTrackBE.Tests.Integration.ApiEndpoints;

public class FilmApiEndpointsTests3
{
    [Fact]
    public async Task GetAllComments__Should_Return_OkResult_With_Enumerable_CommentsInclRatingDto()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();

        var user = await Fakers.User.GenerateOneAndSaveAsync(setup.Context);
        var film = await Fakers.Film.GenerateOneAndSaveAsync(setup.Context);
        var comments = await Fakers.Comment
            .RuleFor(fm => fm.AutorId, f => user.Id)
            .RuleFor(fm => fm.FilmId, f => film.Id)
            .RuleFor(fm => fm.Rating, f => new())
            .GenerateAndSaveAsync(3, setup.Context);

        // Act
        var result = await setup.Controller.GetAllComments(film.Id);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var commentWithRatingDtoList = okResult.Value.Should().BeAssignableTo<IEnumerable<CommentWithRatingDto>>().Subject;

        commentWithRatingDtoList.Should().HaveCount(3);
        commentWithRatingDtoList.Should().OnlyContain(c => c.AutorName == user.UserName);
        commentWithRatingDtoList.Should().OnlyContain(c => c.FilmId == film.Id);
        commentWithRatingDtoList.Should().BeEquivalentTo(comments, o => o
                                         .Including(p => p.Text)
                                         .Including(p => p.SendDate));
    }

    [Fact]
    public async Task GetAllComments__Should_Return_EmptyList_When_FilmHasNoComments()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();

        var film = await Fakers.Film.GenerateOneAndSaveAsync(setup.Context);

        // Act
        var result = await setup.Controller.GetAllComments(film.Id);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var commentWithRatingDtoList = okResult.Value.Should().BeAssignableTo<IEnumerable<CommentWithRatingDto>>().Subject;

        commentWithRatingDtoList.Should().BeEmpty();
    }


    [Fact]
    public async Task GetAllComments__Should_Return_StatusCode500_When_ErrorOccurredInDb()
    {
        // Arrange
        var mockFilmRepository = new Mock<IRepository<Film>>();
        mockFilmRepository
         .Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Film, bool>>>(), It.IsAny<CancellationToken>()))
         .ThrowsAsync(new Exception("Database error"));

        using var setup = FilmApiControllerTestSetup.Create(filmRepository: mockFilmRepository.Object);

        // Act 
        var result = await setup.Controller.GetAllComments(1);

        // Assert
        result.Should().NotBeNull();
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        statusResult.Value.Should().Be("An error occurred while fetching comments.");

        // Verify method calls
        mockFilmRepository.Verify(x => x.AnyAsync(It.IsAny<Expression<Func<Film, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllComments__Should_Return_BadRequest_When_FilmId_IsWrongFormat()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();
        int wrongIdFormat = 0;

        // Act 
        var result = await setup.Controller.GetAllComments(wrongIdFormat);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        badRequestResult.Value.Should().Be("Film ID must be greater than 0.");
    }

    [Fact]
    public async Task GetAllComments__Should_Return_BadRequest_When_FilmId_NotExistInDb()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();
        int nonExistingId = 999;

        // Act 
        var result = await setup.Controller.GetAllComments(nonExistingId);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        badRequestResult.Value.Should().Be($"Film with ID {nonExistingId} not found.");
    }


    [Fact]
    public async Task GetUserProfilData__Should_Return_OkResult_With_EmptyUserProfilDataDto()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();
        await HttpContextTestSetup.Create().BuildAndSaveAsync(setup.Controller, setup.Context);

        // Act
        var result = await setup.Controller.GetUserProfilData();

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var userDataResult = okResult.Value.Should().BeOfType<UserProfilDataDto>().Subject;

        userDataResult.Should().NotBeNull();
        userDataResult.FavoriteFilmsCount.Should().Be(0);
        userDataResult.TotalComments.Should().Be(0);
    }

    [Fact]
    public async Task GetUserProfilData__Should_Return_OkResult_WithUserProfilDataDto()
    {
        // Include LastFavoriteFilmTitle, FavoriteFilmsCount, FavoriteFilms
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();
        var (user, _, _) = await HttpContextTestSetup.Create().BuildAndSaveAsync(setup.Controller, setup.Context);

        var films = await Fakers.Film.RuleFor(fm => fm.ImageFileName, f => $"Img{f.Random.Number(100)}.jpg").GenerateAndSaveAsync(5, setup.Context);
        var expectedFavFilms = films.Take(4);

        // Last add favorite film
        var latestFavFilmName = expectedFavFilms.Last().Name;

        var favoriteFilmsId = expectedFavFilms.Select(p => p.Id);

        user.FavoriteMovies.AddRange(favoriteFilmsId);
        setup.Context.Update(user);
        await setup.Context.SaveChangesAsync();

        // Act
        var result = await setup.Controller.GetUserProfilData();

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var userDataResult = okResult.Value.Should().BeOfType<UserProfilDataDto>().Subject;

        userDataResult.Should().NotBeNull();
        userDataResult.FavoriteFilmsCount.Should().Be(expectedFavFilms.Count());

        userDataResult.LastFavoriteFilmTitle.Should().Be(latestFavFilmName);

        var returned = userDataResult.FavoriteFilms.Select(p => new { Name = p.Title, ImgPath = p.ImagePath });
        var expected = expectedFavFilms.Select(p => new { Name = p.Name, ImgPath = p.ImageFileName });
        returned.Should().BeEquivalentTo(expected);
    }


    [Fact]
    public async Task GetUserProfilData__Should_Return_StatusCode500_When_ErrorOccurredInDb()
    {
        // Arrange
        var mockUserRepository = new Mock<IRepository<ApplicationUser>>();
        mockUserRepository
            .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        using var setup = FilmApiControllerTestSetup.Create(userRepository: mockUserRepository.Object);
        HttpContextTestSetup.Create().Build(setup.Controller);

        // Act 
        var result = await setup.Controller.GetUserProfilData();

        // Assert
        result.Should().NotBeNull();
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        statusResult.Value.Should().Be("An error occurred while fetching user profile data.");

        // Verify that the mocked method was called
        mockUserRepository.Verify(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserProfilData__Should_Return_UnAuthorized_When_UserClaims_AreInvalid()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();

        // Act
        var result = await setup.Controller.GetUserProfilData();

        // Assert
        result.Should().NotBeNull();
        var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        unauthorizedResult.Value.Should().Be("User not authenticated!");
    }

    [Fact]
    public async Task GetUserProfilData__Should_Return_UnAuthorized_When_User_NotExist_InDb()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();
        HttpContextTestSetup.Create()
            .WithUser(userName: "nonexistent@test.com")
            .Build(setup.Controller);

        // Act
        var result = await setup.Controller.GetUserProfilData();

        // Assert
        result.Should().NotBeNull();
        var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        unauthorizedResult.Value.Should().Be("User not authenticated!");
    }

    [Fact]
    public async Task GetUserProfilData__Should_Return_Correct_Rating_And_Comments_Statistics()
    {
        // Arrange
        using var setup = FilmApiControllerTestSetup.Create();
        var (user, _, _) = await HttpContextTestSetup.Create().BuildAndSaveAsync(setup.Controller, setup.Context);

        var films = await Fakers.Film.GenerateAndSaveAsync(3, setup.Context);

        // Create comments with different ratings
        var comments = new List<Comment>
    {
        new() { AutorId = user.Id, FilmId = films[0].Id, Rating =new Rating{ UserRating = 89}, Text = "Great movie", SendDate = DateTime.UtcNow },
        new() { AutorId = user.Id, FilmId = films[1].Id, Rating = new Rating{ UserRating = 60}, Text = "Average movie", SendDate = DateTime.UtcNow },
        new() { AutorId = user.Id, FilmId = films[2].Id, Rating = new Rating{ UserRating = 75}, Text = "Good movie", SendDate = DateTime.UtcNow }
    };

        var expectedAverageRating = (int)comments.Select(p => p.Rating.UserRating).Average();
        var expectedCommentCount = comments.Count;
        var expectedTopRating = comments.Select(p => p.Rating.UserRating).Max();

        await setup.Context.Comments.AddRangeAsync(comments);
        await setup.Context.SaveChangesAsync();

        // Act
        var result = await setup.Controller.GetUserProfilData();

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var userDataResult = okResult.Value.Should().BeOfType<UserProfilDataDto>().Subject;

        userDataResult.Should().NotBeNull();
        userDataResult.TotalComments.Should().Be(expectedCommentCount);
        userDataResult.AvgRating.Should().Be(expectedAverageRating);
        userDataResult.TopRating.Should().Be(expectedTopRating);
    }








}
