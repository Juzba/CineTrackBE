using CineTrackBE.AppServices;
using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CineTrackBE.Tests.Integration.Repositories;

public class RepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly Repository<Film> _repositoryFilm;
    private readonly Repository<IdentityRole> _repositoryRole;

    public RepositoryTests()
    {

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _repositoryFilm = new Repository<Film>(_context, new Mock<ILogger<Repository<Film>>>().Object);
        _repositoryRole = new Repository<IdentityRole>(_context, new Mock<ILogger<Repository<IdentityRole>>>().Object);
    }

    [Fact]
    public async Task AddAsync__Should_AddFilm_ToDb()
    {
        // Arrange
        var film = new Film { Name = "Inception" };

        // Act
        await _repositoryFilm.AddAsync(film);
        await _repositoryFilm.SaveChangesAsync();

        // Assert
        var result = await _repositoryFilm.GetAsync(film.Id);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(film);
    }


    [Fact]
    public async Task AddRangeAsync__Should_AddFilmList_ToDb()
    {
        // Arrange
        var films = new[]
        {
            new Film { Name = "The Matrix" },
            new Film { Name = "Interstellar" },
            new Film { Name = "The Dark Knight" }
        };

        // Act
        await _repositoryFilm.AddRangeAsync(films);
        await _repositoryFilm.SaveChangesAsync();

        // Assert
        var result = await _repositoryFilm.GetList().ToListAsync();

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(films);
        result.Should().HaveCount(3);
    }


    [Fact]
    public async Task GetAsync__IntId_Should_GetFilm_FromDb()
    {
        // Arrange
        var film = new Film { Name = "Matrix" };

        // Act
        await _repositoryFilm.AddAsync(film);
        await _repositoryFilm.SaveChangesAsync();

        // Assert
        var result = await _repositoryFilm.GetAsync(film.Id);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(film);
    }


    [Fact]
    public async Task GetAsync__IntId_Should_BeNull_WhenFilmDoesNotExist()
    {
        // Arrange
        const int nonExistedId = 999;

        // Act
        var result = await _repositoryFilm.GetAsync(nonExistedId);

        // Assert
        result.Should().BeNull("Non-existent film should return null");
    }


    [Fact]
    public async Task GetAsync__StringId_Should_GetFilm_FromDb()
    {
        // Arrange
        var role = new IdentityRole { Name = "Admin" };

        // Act
        await _repositoryRole.AddAsync(role);
        await _repositoryRole.SaveChangesAsync();

        // Assert
        var result = await _repositoryRole.GetAsync(role.Id);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(role);
    }

    [Fact]
    public async Task GetAsync__StringId_Should_BeNull_WhenRoleDoesNotExist()
    {
        // Arrange
        const string nonExistedId = "Non-Existed-Id";

        // Act
        var result = await _repositoryRole.GetAsync(nonExistedId);

        // Assert
        result.Should().BeNull("Non-existent role should return null");
    }

    [Fact]
    public async Task Update__Should_UpdateFilm_InDB()
    {
        // Arrange
        var film = new Film
        {
            Name = "Matrix",
            Description = "Original description",
            Director = "Wachowski"
        };

        await _repositoryFilm.AddAsync(film);
        await _repositoryFilm.SaveChangesAsync();

        // Act
        film.Name = "UpdatedMatrix";
        _repositoryFilm.Update(film);
        await _repositoryFilm.SaveChangesAsync();

        // Assert
        var result = await _repositoryFilm.GetAsync(film.Id);

        result.Should().NotBeNull();
        result.Name.Should().Be("UpdatedMatrix");
        result.Description.Should().Be("Original description");
        result.Director.Should().Be("Wachowski");
    }

    [Fact]
    public async Task Update__Should_ThrowException_WhenFilmDoesNotExist()
    {
        // Arrange
        var nonExistentFilm = new Film
        {
            Id = 99999,
            Name = "NonExistent Film",
            Description = "This film doesn't exist in DB"
        };

        // Pre-condition check
        var exists = await _repositoryFilm.AnyExistsAsync(nonExistentFilm.Id);
        exists.Should().BeFalse();

        // Act & Assert
        _repositoryFilm.Update(nonExistentFilm);

        await FluentActions
              .Awaiting(async () => await _repositoryFilm.SaveChangesAsync())
              .Should()
              .ThrowAsync<ArgumentException>()
              .WithMessage("Attempted to update or delete an entity that does not exist in the store.");
    }

    [Fact]
    public async Task Remove__Should_RemoveFilm_FromDb()
    {
        // Arrange
        var film = new Film { Name = "To Be Deleted" };

        await _repositoryFilm.AddAsync(film);
        await _repositoryFilm.SaveChangesAsync();

        // Check
        var exist = await _repositoryFilm.GetAsync(film.Id);
        exist.Should().BeEquivalentTo(film);

        // Act
        _repositoryFilm.Remove(film);
        await _repositoryFilm.SaveChangesAsync();

        // Assert
        var result = await _repositoryFilm.GetAsync(film.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task Remove__Should_ThrowException_WhenFilmDoesNotExist()
    {
        // Arrange
        var nonExistedFilm = new Film
        {
            Id = 99999,
            Name = "NonExistent Film",
            Description = "This film doesn't exist in DB"
        };

        // Act
        _repositoryFilm.Remove(nonExistedFilm);

        // Assert
        await FluentActions
            .Awaiting(async () => await _repositoryFilm.SaveChangesAsync())
            .Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("Attempted to update or delete an entity that does not exist in the store.");
    }

    [Fact]
    public void RemoveRange__Should_RemoveFilms_FromDb()
    {
        // Arrange
        List<Film> films = 
            [
                new Film
                {
                    Name = "Film 1",
                    Director = "Film 1 Director"
                },
                new Film {
                    Name = "Film 2",
                    Director = "Film 2 Director"
                      },
                new Film {
                    Name = "Film 3",
                    Director = "Film 3 Director"
                }
             ];


        // Check


        // Act

        // Assert






    }






}