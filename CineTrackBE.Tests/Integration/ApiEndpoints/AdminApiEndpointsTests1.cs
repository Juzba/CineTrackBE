using CineTrackBE.AppServices;
using CineTrackBE.Models.DTO;
using CineTrackBE.Models.Entities;
using CineTrackBE.Tests.Helpers.Common;
using CineTrackBE.Tests.Helpers.TestSetups;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace CineTrackBE.Tests.Integration.ApiEndpoints;

public class AdminApiEndpointsTests1
{
    [Fact]
    public async Task AddGenre__Should_Return_CreateAtActionResult_WithObject_GenreDto()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();

        var genreDtoTest = Fakers.GenreDto.Generate();

        // Act
        var result = await setup.Controller.AddGenre(genreDtoTest);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var genreDto = okResult.Value.Should().BeOfType<GenreDto>().Subject;

        genreDto.Should().NotBeNull();
        genreDto.Should().BeEquivalentTo(genreDtoTest, o => o.Excluding(p => p.Id));
    }

    [Fact]
    public async Task AddGenre__Should_AddGenre_ToDb()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();

        var genreDto = Fakers.GenreDto.Generate();

        // Act
        await setup.Controller.AddGenre(genreDto);
        var result = await setup.GenreRepository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Should().BeEquivalentTo(genreDto, o => o.Excluding(p => p.Id));
    }


    [Fact]
    public async Task AddGenre__Should_Return_BadRequest_When_ModelState_IsInvalid()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();
        var genreDto = new GenreDto { Name = string.Empty }; // Invalid name

        setup.Controller.ModelState.AddModelError("Name", "Required");

        // Act
        var result = await setup.Controller.AddGenre(genreDto);
        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
    }

    [Fact]
    public async Task AddGenre__Should_Return_Status500InternalServerError_When_DbError_Occurs()
    {
        // Arrange
        var genreRepositoryMock = new Mock<IRepository<Genre>>();
        genreRepositoryMock.Setup(o => o.AnyAsync(It.IsAny<Expression<Func<Genre, bool>>>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Database error -> genreRepository 'GetList'"));

        using var setup = AdminApiControllerTestSetup.Create(genreRepository: genreRepositoryMock.Object);
        var genreDto = Fakers.GenreDto.Generate();

        // Act
        var result = await setup.Controller.AddGenre(genreDto);

        // Assert
        result.Should().NotBeNull();
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        genreRepositoryMock.Verify(o => o.AnyAsync(It.IsAny<Expression<Func<Genre, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task AddGenre__Should_Return_Conflict_When_Genre_WithSameName_Exists()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();
        var existingGenre = await Fakers.Genre.GenerateOneAndSaveAsync(setup.Context);

        var genreDto = new GenreDto { Name = existingGenre.Name };

        // Act
        var result = await setup.Controller.AddGenre(genreDto);

        // Assert
        result.Should().NotBeNull();
        var conflictResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.Value.Should().Be($"Genre '{existingGenre.Name}' already Exist");
    }


    [Fact]
    public async Task DeleteGenre__Should_DeleteGenre_AndReturn_OkStatus()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();
        var existingGenre = await Fakers.Genre.GenerateOneAndSaveAsync(setup.Context);

        // Act
        var result = await setup.Controller.DeleteGenre(existingGenre.Id);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be($"Genre '{existingGenre.Name}' deleted");
    }

    [Fact]
    public async Task DeleteGenre__Should_DeleteGenre_FromDb()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();
        var existingGenre = await Fakers.Genre.GenerateOneAndSaveAsync(setup.Context);

        // Act
        await setup.Controller.DeleteGenre(existingGenre.Id);
        var result = await setup.GenreRepository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteGenre__Should_Return_BadRequest_When_Id_IsInvalid()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();
        var invalidId = 0;

        // Act
        var result = await setup.Controller.DeleteGenre(invalidId);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Invalid genre ID. ID must be greater than 0.");
    }


    [Fact]
    public async Task DeleteGenre__Should_Return_Conflict_When_Genre_NotExist()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();
        var nonExistentId = 999;

        // Act
        var result = await setup.Controller.DeleteGenre(nonExistentId);

        // Assert
        result.Should().NotBeNull();
        var conflictResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        conflictResult.Value.Should().Be($"Genre with Id '{nonExistentId}' not exist!");
    }


    [Fact]
    public async Task DeleteGenre__Should_Return_Conflict_When_Genre_IsUsedInFilm()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();
        var existingGenre = await Fakers.Genre.GenerateOneAndSaveAsync(setup.Context);
        var film = Fakers.Film.Generate();
        film.FilmGenres = new List<FilmGenre>
        {
            new FilmGenre { GenreId = existingGenre.Id, Film = film }
        };
        await setup.FilmRepository.AddAsync(film);
        await setup.FilmRepository.SaveChangesAsync();

        // Act
        var result = await setup.Controller.DeleteGenre(existingGenre.Id);

        // Assert
        result.Should().NotBeNull();
        var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.Value.Should().Be($"Genre '{existingGenre.Name}' cannot be deleted because it is used!");
    }

    [Fact]
    public async Task DeleteGenre__Should_Return_Status500InternalServerError_When_DbError_Occurs()
    {
        // Arrange
        var genreRepositoryMock = new Mock<IRepository<Genre>>();
        genreRepositoryMock.Setup(o => o.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Database error -> genreRepository 'GetAsync'"));

        using var setup = AdminApiControllerTestSetup.Create(genreRepository: genreRepositoryMock.Object);
        var genreId = 1;

        // Act
        var result = await setup.Controller.DeleteGenre(genreId);

        // Assert
        result.Should().NotBeNull();
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        genreRepositoryMock.Verify(o => o.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // EDIT GENRE //
    [Fact]
    public async Task EditGenre__Should_ReturnOkObjectResult_withEdited_GenreDto()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();

        var existGenre = await Fakers.Genre.RuleFor(fm => fm.Name, f => "Genre_Test_Name").GenerateOneAndSaveAsync(setup.Context);
        var targetId = existGenre.Id;

        var genreDto = new GenreDto { Name = "Edited_Genre_Test_Name" };

        // Act
        var result = await setup.Controller.EditGenre(targetId, genreDto);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var genreDtoResult = okResult.Value.Should().BeOfType<GenreDto>().Subject;

        genreDtoResult.Should().NotBeNull();
        genreDtoResult.Name.Should().Be(genreDto.Name);
    }

    [Fact]
    public async Task EditGenre__Should_EditGenre_InsideDb()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();
        var existGenre = await Fakers.Genre.RuleFor(fm => fm.Name, f => "Genre_Test_Name").GenerateOneAndSaveAsync(setup.Context);
        var targetId = existGenre.Id;

        // Check
        var check = await setup.GenreRepository.GetAllAsync();
        check.Should().ContainSingle();
        check.First().Name.Should().Be(existGenre.Name);

        var genreDto = new GenreDto { Name = "Edited_Genre_Test_Name" };

        // Act
        await setup.Controller.EditGenre(targetId, genreDto);
        var result = await setup.GenreRepository.GetAsync(targetId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(genreDto.Name);
    }

    [Fact]
    public async Task EditGenre__Should_Return_BadRequest_When_Id_IsInvalid()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();
        var invalidId = 0;

        // Act
        var result = await setup.Controller.EditGenre(invalidId, new GenreDto());

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Invalid genre ID. ID must be greater than 0.");
    }

    [Fact]
    public async Task EditGenre__Should_Return_BadRequest_When_ModelState_IsInvalid()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();

        var genreDto = new GenreDto { Name = string.Empty }; // Invalid name
        setup.Controller.ModelState.AddModelError("Name", "Required");

        // Act
        var result = await setup.Controller.EditGenre(1, genreDto);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value!.ToString().Should().Contain("Genre is not valid");
    }


    [Fact]
    public async Task EditGenre__Should_Return_Conflict_When_Genre_WithSameName_Exists()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();
        var existingGenre1 = await Fakers.Genre.RuleFor(fm => fm.Name, f => "Genre_Test_Name_1").GenerateOneAndSaveAsync(setup.Context);
        var existingGenre2 = await Fakers.Genre.RuleFor(fm => fm.Name, f => "Genre_Test_Name_2").GenerateOneAndSaveAsync(setup.Context);
        var genreDto = new GenreDto { Name = existingGenre2.Name };

        // Act
        var result = await setup.Controller.EditGenre(existingGenre1.Id, genreDto);

        // Assert
        result.Should().NotBeNull();
        var conflictResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.Value.Should().Be($"Genre NAME '{genreDto.Name}' already exist!");
    }


    [Fact]
    public async Task EditGenre_Should_Return_NotFound_When_Genre_NotExist()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();
        var nonExistentId = 999;
        var genreDto = new GenreDto { Name = "Some_Name" };

        // Act
        var result = await setup.Controller.EditGenre(nonExistentId, genreDto);

        // Assert
        result.Should().NotBeNull();
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be($"Genre with Id '{nonExistentId}' not found!");
    }

    [Fact]
    public async Task EditGenre__Should_Return_Status500InternalServerError_When_DbError_Occurs()
    {
        // Arrange
        var genreRepositoryMock = new Mock<IRepository<Genre>>();
        genreRepositoryMock.Setup(o => o.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Database error -> genreRepository 'GetAsync'"));

        using var setup = AdminApiControllerTestSetup.Create(genreRepository: genreRepositoryMock.Object);
        var genreDto = new GenreDto { Name = "Some_Name" };

        // Act
        var result = await setup.Controller.EditGenre(1, genreDto);

        // Assert
        result.Should().NotBeNull();
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        genreRepositoryMock.Verify(o => o.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // GET ALL FILMS //
    [Fact]
    public async Task GetAllFilms__Should_ReturnOkObjectResult_WithEnumerable_FilmDto()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();

        var films = await Fakers.FilmIncGenre.GenerateAndSaveAsync(3, setup.Context);

        // Act
        var result = await setup.Controller.GetAllFilms();

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<FilmDto>>();
    }


    [Fact]
    public async Task GetAllFilms__Should_ReturnEnumerable_FilmDto_IncludeGenres()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();
        var films = await Fakers.FilmIncGenre.GenerateAndSaveAsync(3, setup.Context);

        // Act
        var result = await setup.Controller.GetAllFilms();

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var filmDtos = okResult.Value.Should().BeAssignableTo<IEnumerable<FilmDto>>().Subject;

        filmDtos.Should().HaveCount(films.Count);
        filmDtos.Should().OnlyContain(p => p.Genres != null && p.Genres.Count > 0);
        filmDtos.Should().OnlyContain(p => p.Genres.First().Name.Contains("Genre"));
    }


    [Fact]
    public async Task GetAllFilms__Should_ReturnOkStatus_When_ZeroFilm_Exist()
    {
        // Arrange
        using var setup = AdminApiControllerTestSetup.Create();

        // Act
        var result = await setup.Controller.GetAllFilms();

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var filmDtos = okResult.Value.Should().BeAssignableTo<IEnumerable<FilmDto>>().Subject;
        filmDtos.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllFilms__Should_Return_Status500InternalServerError_When_DbError_Occurs()
    {
        // Arrange
        var filmRepositoryMock = new Mock<IRepository<Film>>();
        filmRepositoryMock.Setup(o => o.GetList())
        .Throws(new Exception("Database error -> filmRepository 'GetList'"));

        using var setup = AdminApiControllerTestSetup.Create(filmRepository: filmRepositoryMock.Object);

        // Act
        var result = await setup.Controller.GetAllFilms();

        // Assert
        result.Should().NotBeNull();
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        filmRepositoryMock.Verify(o => o.GetList(), Times.Once);
    }

}
