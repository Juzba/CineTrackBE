using CineTrackBE.AppServices;
using CineTrackBE.Models.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace CineTrackBE.Tests.Unit.Repositories;

public class FilmRepositoryTests
{
    private readonly Mock<IRepository<Film>> _mockRepository;
    private readonly Film _testFilm;



    public FilmRepositoryTests()
    {
        _mockRepository = new Mock<IRepository<Film>>();


        _testFilm = new Film
        {
            Id = 1,
            Name = "Test Film",
            Director = "Test Director",
            ReleaseDate = DateTime.Now,
            Description = "Test Description"
        };
    }


    //[Fact]
    //public async Task GetAsync_Id_ReturnsFilm_WhenFilmExists()
    //{
    //    // Arrange

    //    _mockRepository.Setup(repo => repo.GetAsync_Id(1, default))
    //        .ReturnsAsync(_testFilm);

    //    // Act
    //    var result = await _mockRepository.Object.GetAsync_Id(1);

    //    // Assert
    //    result.Should().NotBeNull();
    //    result.Should().BeEquivalentTo(_testFilm, options => options
    //        .Including(x => x.Id)
    //        .Including(x => x.Name)
    //        .Including(x => x.Director)
    //        .Including(x => x.Description));
    //}

    //[Fact]
    //public async Task GetAsync_Id_ReturnsNull_WhenFilmDoesNotExist()
    //{
    //    // Arrange
    //    _mockRepository.Setup(repo => repo.GetAsync_Id(999, default))
    //        .ReturnsAsync((Film)null!);

    //    // Act
    //    var result = await _mockRepository.Object.GetAsync_Id(999);

    //    // Assert
    //    result.Should().BeNull();
    //}

    //[Fact]
    //public async Task GetAsync_Id_String_ReturnIdentityRole_WhenExist()
    //{
    //    // Arrange
    //    var mockRepositoryIR = new Mock<IRepository<IdentityRole>>();
    //    var expectedFilm = new IdentityRole
    //    {
    //        Id = "TestId",
    //        Name = "Test Film",
    //    };

    //    mockRepositoryIR
    //        .Setup(r => r.GetAsync_Id("TestId", default))
    //        .ReturnsAsync(expectedFilm);


    //    // Act
    //    var result = await mockRepositoryIR.Object.GetAsync_Id("TestId", default);


    //    // Assert
    //    result.Should().NotBeNull();
    //    result.Should().BeEquivalentTo(expectedFilm, options => options
    //            .Including(p => p.Id)
    //            .Including(p => p.Name)

    //        );
    //}


    //[Fact]
    //public async Task GetAsync_Id_String_ReturnNull_When_FilmNotExist()
    //{
    //    // Arrange
    //    var mockRepositoryIR = new Mock<IRepository<IdentityRole>>();

    //    mockRepositoryIR
    //        .Setup(r => r.GetAsync_Id("Test", default))
    //        .ReturnsAsync((IdentityRole)null!);

    //    // Act
    //    var result = await mockRepositoryIR.Object.GetAsync_Id("Test", default);


    //    // Assert
    //    result.Should().BeNull();
    //}

    //[Fact]
    //public async Task GetAsync_Id_String_ThrowException_When_IdIsNull()
    //{
    //    // Arrange
    //    var mockRepositoryIR = new Mock<IRepository<IdentityRole>>();
    //    mockRepositoryIR.Setup(p => p.GetAsync_Id(null!, default))
    //        .ThrowsAsync(new ArgumentException(nameof(IRepository<IdentityRole>.GetAsync_Id)));

    //    // Act & Assert
    //    await mockRepositoryIR.Object.Invoking(x => x.GetAsync_Id(null!, default))
    //        .Should()
    //        .ThrowAsync<ArgumentException>()
    //        .WithMessage("*GetAsync_Id*");
    //}

























}