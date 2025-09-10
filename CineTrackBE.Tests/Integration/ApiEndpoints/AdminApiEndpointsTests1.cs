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
        var genreRepositoryMock = new Mock<Repository<Genre>>();
        genreRepositoryMock.Setup(o => o.GetList()).Throws(new Exception("Database error -> genreRepository 'GetList'"));

        using var setup = AdminApiControllerTestSetup.Create(genreRepository: genreRepositoryMock.Object);
        var genreDto = Fakers.GenreDto.Generate();

        // Act
        var result = await setup.Controller.AddGenre(genreDto);

        // Assert
        result.Should().NotBeNull();
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        genreRepositoryMock.Verify(o => o.GetList(), Times.Once);
    }











    // AddGenre db error 500








    // db error 500
    // if user is not admin return problem
    // genre with same name exist return conflict


















}
