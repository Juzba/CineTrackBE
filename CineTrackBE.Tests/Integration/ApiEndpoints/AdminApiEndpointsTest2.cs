using CineTrackBE.AppServices;
using CineTrackBE.Models.DTO;
using CineTrackBE.Models.Entities;
using CineTrackBE.Tests.Helpers.Common;
using CineTrackBE.Tests.Helpers.TestSetups;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace CineTrackBE.Tests.Integration.ApiEndpoints;

public class AdminApiEndpointsTest2
{
    // ADD FILM //
    [Fact]
    public async Task AddFilm__ShouldReturnCreatedAtActionResult_WithFilmDto()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();

        var genres = await Fakers.Genre.GenerateAndSaveAsync(2, setup.Context);
        var genresDto = genres.Select(p => new GenreDto { Id = p.Id, Name = p.Name });

        var filmDto = Fakers.FilmDto.Generate();
        filmDto.Genres.AddRange(genresDto);

        // Act
        var result = await setup.Controller.AddFilm(filmDto);

        // Assert
        var okResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var filmDtoResult = okResult.Value.Should().BeOfType<FilmDto>().Subject;
        filmDtoResult.Should().BeEquivalentTo(filmDto, o => o.Excluding(p => p.Id));
    }


    [Fact]
    public async Task AddFilm__ShouldAddFilmToDb_IncludingFilmGenres()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();

        var genres = await Fakers.Genre.GenerateAndSaveAsync(2, setup.Context);
        var genresDto = genres.Select(p => new GenreDto { Id = p.Id, Name = p.Name });

        var filmDto = Fakers.FilmDto.Generate();
        filmDto.Genres.AddRange(genresDto);

        // Act
        var result = await setup.Controller.AddFilm(filmDto);
        var filmsInDb = await setup.FilmRepository.GetAllAsync();
        var filmGenresInDb = await setup.FilmGenreRepository.GetAllAsync();

        // Assert
        filmsInDb.Should().NotBeNull();
        filmsInDb.Should().ContainSingle();
        filmsInDb.First().Director.Should().Be(filmDto.Director);

        filmGenresInDb.Should().NotBeNull();
        filmGenresInDb.Should().HaveCount(genres.Count);

        var filmId = filmsInDb.First().Id;
        filmGenresInDb.Should().OnlyContain(p => p.FilmId == filmId);
    }

    [Fact]
    public async Task AddFilm__ShouldReturnBadRequest_WhenGenreNotExistInDb()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();
        var genresDto = Fakers.GenreDto.RuleFor(fm => fm.Id, f => f.IndexFaker).Generate(2);
        var filmDto = Fakers.FilmDto.Generate();
        filmDto.Genres.AddRange(genresDto);

        // Act
        var result = await setup.Controller.AddFilm(filmDto);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be($"Genre with Id '{genresDto[0].Id}' does not exist in db!");
    }

    [Fact]
    public async Task AddFilm__ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();

        var filmDto = Fakers.FilmDto.Generate();
        filmDto.Name = null!; // Name is required, setting it to null to invalidate the model state

        setup.Controller.ModelState.AddModelError("Name", "The Name field is required.");

        // Act
        var result = await setup.Controller.AddFilm(filmDto);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().BeOfType<SerializableError>();
    }


    [Fact]
    public async Task AddFilm__ShouldReturn_Status500InternalServerError_WhenDbErrorOccured()
    {
        // Arrange
        var filmRepositoryMock = new Mock<IRepository<Film>>();

        filmRepositoryMock.Setup(p => p.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mock<IDbContextTransaction>().Object);

        filmRepositoryMock.Setup(p => p.AnyAsync(It.IsAny<Expression<Func<Film, bool>>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error test"));

        using var setup = AdminApiControllerTestSetup.Create(filmRepository: filmRepositoryMock.Object);
        var filmDto = Fakers.FilmDto.Generate();

        // Act
        var result = await setup.Controller.AddFilm(filmDto);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        objectResult.Value.Should().Be("Error occurred while trying to save Film to db");
    }


    [Fact]
    public async Task AddFilm__ShouldRollbackTransaction_WhenErrorOccursWhileSavingFilmGenre()
    {
        // Arrange
        var filmGenreRepositoryMock = new Mock<IRepository<FilmGenre>>();
        filmGenreRepositoryMock.Setup(p => p.AddRangeAsync(It.IsAny<IEnumerable<FilmGenre>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error test to rollback data. FilmGenre -> AddRangeAsync"));

        using var setup = AdminApiControllerTestSetup.Create(filmGenreRepository: filmGenreRepositoryMock.Object);

        var genres = await Fakers.Genre.GenerateAndSaveAsync(2, setup.Context);
        var genresDto = genres.Select(p => new GenreDto { Id = p.Id, Name = p.Name });

        var filmDto = Fakers.FilmDto.Generate();
        filmDto.Genres.AddRange(genresDto);

        // Act
        var result = await setup.Controller.AddFilm(filmDto);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        objectResult.Value.Should().Be("Error occurred while trying to save Film to db");

        var filmsInDb = await setup.FilmRepository.GetAllAsync();
        filmsInDb.Should().BeEmpty();

        var filmGenresInDb = await setup.FilmGenreRepository.GetAllAsync();
        filmGenresInDb.Should().BeEmpty();
    }

    // DELETE FILM //
    [Fact]
    public async Task DeleteFilm__ShouldReturnOkResult_WhenFilmIsDeletedFromDb()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();
        var film = await Fakers.Film.GenerateOneAndSaveAsync(setup.Context);
        var filmId = film.Id;

        var check = await setup.FilmRepository.GetAllAsync();
        check.Should().ContainSingle();

        // Act
        var result = await setup.Controller.DeleteFilm(filmId);

        // Assert
        var okResult = result.Should().BeOfType<OkResult>().Subject;
        var filmInDb = await setup.FilmRepository.GetAllAsync();
        filmInDb.Should().BeEmpty();


    }

    [Fact]
    public async Task DeleteFilm__ShouldDeleteFilmIncludingRelatedEntities_FromDb()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();

        var film = await Fakers.Film.GenerateOneAndSaveAsync(setup.Context);
        var filmId = film.Id;

        var genres = await Fakers.Genre.GenerateAndSaveAsync(2, setup.Context);
        var filmGenres = genres.Select(g => new FilmGenre { FilmId = filmId, GenreId = g.Id }).ToList();

        await setup.FilmGenreRepository.AddRangeAsync(filmGenres);
        await setup.FilmGenreRepository.SaveChangesAsync();

        var comments = await Fakers.Comment.RuleFor(c => c.FilmId, filmId).GenerateAndSaveAsync(2, setup.Context);
        var rating = await Fakers.Rating.RuleFor(r => r.CommentId, comments[0].Id).GenerateOneAndSaveAsync(setup.Context);


        // Pre-check
        var filmCheck = await setup.FilmRepository.GetAllAsync();
        filmCheck.Should().ContainSingle();

        var filmGenreCheck = await setup.FilmGenreRepository.GetAllAsync();
        filmGenreCheck.Should().HaveCount(genres.Count); // 2

        var ratingCheck = await setup.Context.Ratings.ToListAsync();
        ratingCheck.Should().ContainSingle();

        var commentsCheck = await setup.Context.Comments.ToListAsync();
        commentsCheck.Should().HaveCount(comments.Count); // 2

        // Act
        var result = await setup.Controller.DeleteFilm(filmId);

        // Assert
        var okResult = result.Should().BeOfType<OkResult>().Subject;
        (await setup.FilmRepository.GetAllAsync()).Should().BeEmpty();
        (await setup.FilmGenreRepository.GetAllAsync()).Should().BeEmpty();
        (await setup.Context.Ratings.ToListAsync()).Should().BeEmpty();
        (await setup.Context.Comments.ToListAsync()).Should().BeEmpty();
    }


    [Fact]
    public async Task DeleteFilm__ShouldReturn_Status500InternalServerError_WhenDbErrorOccured()
    {
        // Arrange
        var filmRepositoryMock = new Mock<IRepository<Film>>();
        filmRepositoryMock.Setup(p => p.GetList())
            .Throws(new Exception("Database error test -> MockFilmRepository > GetList!"));

        using var setup = AdminApiControllerTestSetup.Create(filmRepository: filmRepositoryMock.Object);

        // Act
        var result = await setup.Controller.DeleteFilm(1);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        objectResult.Value.Should().Be("Error occurred while trying to save Film to db");
    }

    [Fact]
    public async Task DeleteFilm__ShouldReturnBadRequest_WhenIdIsInvalid()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();
        int invalidId = 0;

        // Act
        var result = await setup.Controller.DeleteFilm(invalidId);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Invalid film ID. ID must be greater than 0.");
    }

    [Fact]
    public async Task DeleteFilm__ShouldReturnNotFound_WhenFilmIsNotInDb()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();
        int nonExistentId = 999;

        // Act
        var result = await setup.Controller.DeleteFilm(nonExistentId);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be($"Film with Id '999' not exist!");
    }

    // EDIT FILM //
    [Fact]
    public async Task EditFilm__ShouldReturnOkResult_WithEditedFilmDto()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();

        var genres = await Fakers.Genre.GenerateAndSaveAsync(2, setup.Context);
        var genresDto = genres.Select(p => new GenreDto { Id = p.Id, Name = p.Name });

        var film = await Fakers.Film.GenerateOneAndSaveAsync(setup.Context);

        var filmDto = new FilmDto
        {
            Id = film.Id,
            Name = "Edited Name",
            Description = "Edited Description",
            Director = "Edited Director",
            ImageFileName = "edited_image.jpg",
            ReleaseDate = DateTime.UtcNow.AddYears(-1),
            Genres = [.. genresDto]
        };

        // Act
        var result = await setup.Controller.EditFilm(filmDto.Id, filmDto);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var filmDtoResult = okResult.Value.Should().BeOfType<FilmDto>().Subject;

        filmDtoResult.Should().NotBeNull();
        filmDtoResult.Should().BeEquivalentTo(filmDto);
    }

    [Fact]
    public async Task EditFilm__ShouldEditFilm_InDB()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();

        var genres = await Fakers.Genre.GenerateAndSaveAsync(2, setup.Context);
        var genresDto = genres.Select(p => new GenreDto { Id = p.Id, Name = p.Name });

        var film = await Fakers.Film.RuleFor(fm => fm.Name, f => "Test Film").GenerateOneAndSaveAsync(setup.Context);

        var filmDto = new FilmDto
        {
            Id = film.Id,
            Name = "Edited Name",
            Director = "Edited Director",
            Genres = [.. genresDto]
        };

        // Act
        var result = await setup.Controller.EditFilm(filmDto.Id, filmDto);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;

        var checkResult = await setup.FilmRepository.GetAllAsync();
        checkResult.Should().ContainSingle();
        checkResult.First().Name.Should().Be(filmDto.Name);
        checkResult.First().Director.Should().Be(filmDto.Director);
    }


    [Fact]
    public async Task EditFilm__ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();

        var filmDto = new FilmDto
        {
            Name = null!, // Name is required, setting it to null to invalidate the model state
            Director = "Edited Director",
            Genres = []
        };
        setup.Controller.ModelState.AddModelError("Name", "The Name field is required.");

        // Act
        var result = await setup.Controller.EditFilm(1, filmDto);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value!.ToString().Should().StartWith("Film is not valid");
    }

    [Fact]
    public async Task EditFilm__ShouldReturnBadRequest_WhenIdIsInvalid()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();
        var filmDto = Fakers.FilmDto.Generate();
        int InvalidId = 0;

        // Act
        var result = await setup.Controller.EditFilm(InvalidId, filmDto);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Invalid film ID. ID must be greater than 0.");
    }

    [Fact]
    public async Task EditFilm__ShouldReturn_Status500InternalServerError_WhenDbErrorOccured()
    {
        // Arrange
        var filmRepositoryMock = new Mock<IRepository<Film>>();

        filmRepositoryMock.Setup(p => p.BeginTransactionAsync(It.IsAny<CancellationToken>()))
          .ReturnsAsync(new Mock<IDbContextTransaction>().Object);

        filmRepositoryMock.Setup(p => p.GetList())
            .Throws(new Exception("Database error test -> filmRepositoryMock > GetList()"));

        using var setup = AdminApiControllerTestSetup.Create(filmRepository: filmRepositoryMock.Object);
        var filmDto = Fakers.FilmDto.Generate();

        // Act
        var result = await setup.Controller.EditFilm(1, filmDto);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Error occurred while trying to save Film to db");
    }

    [Fact]
    public async Task EditFilm__ShouldReturnNotFound_WhenFilmNotExist()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();

        var nonExistedfilmDto = Fakers.FilmDto.RuleFor(fm => fm.Id, f => f.IndexFaker + 1).Generate();

        // Act
        var result = await setup.Controller.EditFilm(nonExistedfilmDto.Id, nonExistedfilmDto);

        // Assert
        result.Should().NotBeNull();
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be($"Film with Id '{nonExistedfilmDto.Id}' not found!");
    }


    [Fact]
    public async Task EditFilm__ShouldEditFilmGenres_InDB()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();

        var initialGenres = await Fakers.Genre.GenerateAndSaveAsync(2, setup.Context);
        var newGenres = await Fakers.Genre.GenerateAndSaveAsync(2, setup.Context);

        var film = await Fakers.Film.GenerateOneAndSaveAsync(setup.Context);
        var filmId = film.Id;

        var initialFilmGenres = initialGenres.Select(g => new FilmGenre { FilmId = filmId, GenreId = g.Id }).ToList();

        await setup.FilmGenreRepository.AddRangeAsync(initialFilmGenres);
        await setup.FilmGenreRepository.SaveChangesAsync();

        var newGenresDto = newGenres.Select(p => new GenreDto { Id = p.Id, Name = p.Name });
        var filmDto = new FilmDto
        {
            Id = film.Id,
            Name = "Edited Name",
            Director = "Edited Director",
            Genres = [.. newGenresDto]
        };

        // Act
        var result = await setup.Controller.EditFilm(filmDto.Id, filmDto);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;

        var filmGenresInDb = await setup.FilmGenreRepository.GetAllAsync();
        filmGenresInDb.Should().HaveCount(newGenres.Count); // 2
        filmGenresInDb.Should().OnlyContain(p => p.FilmId == filmId);
        filmGenresInDb.Select(p => p.GenreId).Should().BeEquivalentTo(newGenres.Select(g => g.Id));
    }

}
