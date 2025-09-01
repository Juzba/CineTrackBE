using CineTrackBE.ApiControllers;
using CineTrackBE.AppServices;
using CineTrackBE.Models.DTO;
using CineTrackBE.Models.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Xunit;
//using Xunit;

namespace CineTrackBE.Tests.Unit.Controllers;

public class FilmApiControllerTests
{
    private readonly Mock<ILogger<FilmApiController>> _loggerMock;
    private readonly Mock<IRepository<Film>> _filmRepositoryMock;
    private readonly Mock<IRepository<Rating>> _ratingRepositoryMock;
    private readonly Mock<IRepository<Comment>> _commentRepositoryMock;
    private readonly Mock<IRepository<ApplicationUser>> _userRepositoryMock;
    private readonly Mock<IRepository<Genre>> _genreRepositoryMock;
    private readonly Mock<IDataService> _dataServiceMock;
    private readonly FilmApiController _controller;

    public FilmApiControllerTests()
    {
        _loggerMock = new Mock<ILogger<FilmApiController>>();
        _filmRepositoryMock = new Mock<IRepository<Film>>();
        _ratingRepositoryMock = new Mock<IRepository<Rating>>();
        _commentRepositoryMock = new Mock<IRepository<Comment>>();
        _userRepositoryMock = new Mock<IRepository<ApplicationUser>>();
        _genreRepositoryMock = new Mock<IRepository<Genre>>();
        _dataServiceMock = new Mock<IDataService>();

        _controller = new FilmApiController(
            _loggerMock.Object,
            _filmRepositoryMock.Object,
            _ratingRepositoryMock.Object,
            _commentRepositoryMock.Object,
            _userRepositoryMock.Object,
            _genreRepositoryMock.Object,
            _dataServiceMock.Object
        );
    }

    [Fact]
    public async Task GetLatestFilms_ShouldReturnTop5Films()
    {
        // Arrange
        var genres = new List<Genre>
        {
            new() { Id = 1, Name = "Action" },
            new() { Id = 2, Name = "Drama" }
        };

        var films = new List<Film>
        {
            new()
            {
                Id = 1,
                Name = "Film 1",
                ReleaseDate = DateTime.Now.AddDays(-1),
                FilmGenres = new List<FilmGenre>
                {
                    new()
                    {
                        FilmId = 1,
                        GenreId = 1,
                        Genre = genres[0]
                    }
                }
            },
            new()
            {
                Id = 2,
                Name = "Film 2",
                ReleaseDate = DateTime.Now.AddDays(-2),
                FilmGenres = new List<FilmGenre>
                {
                    new()
                    {
                        FilmId = 2,
                        GenreId = 2,
                        Genre = genres[1]
                    }
                }
            }
        };

        // Správná syntaxe pro MockQueryable
        var mockFilms = films.AsQueryable();

        _filmRepositoryMock
            .Setup(x => x.GetList())
            .Returns(mockFilms);
        // Act
        var result = await _controller.GetLatestFilms();

        // Assert
        result.Should().NotBeNull();
        result.Value.Should()
            .NotBeNull()
            .And.HaveCount(2)
            .And.SatisfyRespectively(
                first =>
                {
                    first.Id.Should().Be(1);
                    first.Name.Should().Be("Film 1");
                    first.Genres.Should().HaveCount(1);
                    first.Genres.First().Id.Should().Be(1);
                    first.Genres.First().Name.Should().Be("Action");
                },
                second =>
                {
                    second.Id.Should().Be(2);
                    second.Name.Should().Be("Film 2");
                    second.Genres.Should().HaveCount(1);
                    second.Genres.First().Id.Should().Be(2);
                    second.Genres.First().Name.Should().Be("Drama");
                }
            );

        // Ověření že filmy jsou seřazeny podle data (nejnovější první)
        result.Value.Should().BeInDescendingOrder(f => f.ReleaseDate);
    }






















}
