using CineTrackBE.Models.Entities;
using CineTrackBE.Tests.Helpers.Common;
using CineTrackBE.Tests.Helpers.TestSetups;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CineTrackBE.Tests.Integration.Repositories;

public class RepositoryTests
{
    [Fact]
    public async Task AddAsync__Should_AddFilm_ToDb()
    {
        // Arrange
        using var setup = RepositoryTestSetup.Create();
        var filmRepository = setup.FilmRepository;

        var film = Fakers.Film.Generate();

        // Act
        await filmRepository.AddAsync(film);
        await filmRepository.SaveChangesAsync();

        // Assert
        var result = await filmRepository.GetAsync(film.Id);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(film);
    }


    [Fact]
    public async Task AddRangeAsync__Should_AddFilmList_ToDb()
    {
        // Arrange
        using var setup = RepositoryTestSetup.Create();
        var filmRepository = setup.FilmRepository;

        var films = Fakers.Film.Generate(3);

        // Act
        await filmRepository.AddRangeAsync(films);
        await filmRepository.SaveChangesAsync();

        // Assert
        var result = await filmRepository.GetList().ToListAsync();

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(films);
        result.Should().HaveCount(3);
    }


    [Fact]
    public async Task GetAsync__IntId_Should_GetFilm_FromDb()
    {
        // Arrange
        using var setup = RepositoryTestSetup.Create();
        var filmRepository = setup.FilmRepository;

        var film = await Fakers.Film.GenerateOneAndSaveAsync(setup.Context);

        // Act
        var result = await filmRepository.GetAsync(film.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(film);
    }


    [Fact]
    public async Task GetAsync__IntId_Should_BeNull_WhenFilmDoesNotExist()
    {
        // Arrange
        using var setup = RepositoryTestSetup.Create();
        var filmRepository = setup.FilmRepository;

        const int nonExistedId = 99;

        // Act
        var result = await filmRepository.GetAsync(nonExistedId);

        // Assert
        result.Should().BeNull("Non-existent film should return null");
    }


    [Fact]
    public async Task GetAsync__StringId_Should_GetRole_FromDb()
    {
        // Arrange
        using var setup = RepositoryTestSetup.Create();
        var roleRepository = setup.RoleRepository;

        var role = await Fakers.Role.GenerateOneAndSaveAsync(setup.Context);

        // Act
        var result = await roleRepository.GetAsync(role.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(role);
    }

    [Fact]
    public async Task GetAsync__StringId_Should_BeNull_WhenRoleDoesNotExist()
    {
        // Arrange
        using var setup = RepositoryTestSetup.Create();
        var roleRepository = setup.RoleRepository;

        const string nonExistedId = "Non-Existed-Id";

        // Act
        var result = await roleRepository.GetAsync(nonExistedId);

        // Assert
        result.Should().BeNull("Non-existent role should return null");
    }

    [Fact]
    public async Task Update__Should_UpdateFilm_InDB()
    {
        // Arrange
        using var setup = RepositoryTestSetup.Create();
        var filmRepository = setup.FilmRepository;

        var originalFilm = new Film { Name = "Test Before Update", Director = "Test Director" };
        await filmRepository.AddAsync(originalFilm);
        await filmRepository.SaveChangesAsync();

        // clear ef
        setup.Context.ChangeTracker.Clear();

        var updatedFilm = new Film
        {
            Id = originalFilm.Id,
            Name = "Test After Update",
            Director = "Test Director"
        };

        // Act
        filmRepository.Update(updatedFilm);
        await filmRepository.SaveChangesAsync();

        // Assert
        var result = await filmRepository.GetAsync(originalFilm.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test After Update");
        result.Director.Should().Be("Test Director");
    }


    [Fact]
    public async Task Update__Should_ThrowException_WhenFilmDoesNotExist()
    {
        // Arrange
        using var setup = RepositoryTestSetup.Create();
        var filmRepository = setup.FilmRepository;

        var nonExistentFilm = Fakers.Film.Generate();
        nonExistentFilm.Id = 999;

        // Pre-condition check
        var exists = await filmRepository.AnyAsync(p => p.Id == nonExistentFilm.Id);
        exists.Should().BeFalse();

        // Act & Assert
        filmRepository.Update(nonExistentFilm);

        await FluentActions
              .Awaiting(async () => await filmRepository.SaveChangesAsync())
              .Should()
              .ThrowAsync<ArgumentException>()
              .WithMessage("The database operation was expected to affect 1 row(s)*");
    }

    [Fact]
    public async Task Remove__Should_RemoveFilm_FromDb()
    {
        // Arrange
        using var setup = RepositoryTestSetup.Create();
        var filmRepository = setup.FilmRepository;

        var film = await Fakers.Film.GenerateOneAndSaveAsync(setup.Context);

        // Check
        var exist = await filmRepository.GetAsync(film.Id);
        exist.Should().BeEquivalentTo(film);

        // Act
        filmRepository.Remove(film);
        await filmRepository.SaveChangesAsync();

        // Assert
        var result = await filmRepository.GetAsync(film.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task Remove__Should_ThrowException_WhenFilmDoesNotExist()
    {
        // Arrange
        using var setup = RepositoryTestSetup.Create();
        var filmRepository = setup.FilmRepository;

        var nonExistedFilm = Fakers.Film.Generate();
        nonExistedFilm.Id = 98;

        // Act
        filmRepository.Remove(nonExistedFilm);

        // Assert
        await FluentActions
            .Awaiting(async () => await filmRepository.SaveChangesAsync())
            .Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("The database operation was expected to affect 1 row(s)*");
    }

    [Fact]
    public async Task RemoveRange__Should_RemoveFilms_FromDb()
    {
        // Arrange
        using var setup = RepositoryTestSetup.Create();
        var filmRepository = setup.FilmRepository;

        var films = await Fakers.Film.GenerateAndSaveAsync(4, setup.Context);

        // Check
        var check = await filmRepository.GetList().ToListAsync();
        check.Should().HaveCount(4);

        // Act
        filmRepository.RemoveRange(films);
        await filmRepository.SaveChangesAsync();

        // Assert
        var result = await filmRepository.GetList().ToListAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveRangeAsync__ThrowException_When_FilmsDoesNotExist_InDb()
    {
        // Arrange
        using var setup = RepositoryTestSetup.Create();
        var filmRepository = setup.FilmRepository;

        // Arrange
        List<Film> NonExistedFilms = [new Film { Id = 100, Name = "NonExistingFilm1" }, new Film { Id = 101, Name = "NonExistingFilm2" }];

        // Act
        filmRepository.RemoveRange(NonExistedFilms);

        // Assert
        await FluentActions
            .Awaiting(async () => await filmRepository.SaveChangesAsync())
            .Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("The database operation was expected to affect 1 row(s)*");
    }

    [Fact]
    public async Task GetList__Should_ReturnQueryableList()
    {
        // Arrange
        using var setup = RepositoryTestSetup.Create();
        var filmRepository = setup.FilmRepository;

        var films = await Fakers.Film.GenerateAndSaveAsync(3, setup.Context);

        // Act & Assert
        var result = filmRepository.GetList();
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IQueryable<Film>>();
        result.Should().HaveCount(3);
    }

   
    [Fact]
    public async Task AnyAsync__Should_ReturnTrue_WhenFilmExists()
    {
        // Arrange
        using var setup = RepositoryTestSetup.Create();
        var filmRepository = setup.FilmRepository;

        var film = await Fakers.Film.GenerateOneAndSaveAsync(setup.Context);

        // Act
        var result = await filmRepository.AnyAsync(p => p.Id == film.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AnyAsync__Should_ReturnFalse_WhenFilmDoesNotExists()
    {
        // Arrange
        using var setup = RepositoryTestSetup.Create();
        var filmRepository = setup.FilmRepository;

        const int NonExistingId = 97;

        // Act
        var result = await filmRepository.AnyAsync(p => p.Id == NonExistingId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SaveChangesAsync__Should_ThrowException_WhenDbUpdateExceptionOccurs()
    {
        // Arrange
        using var setup = RepositoryTestSetup.Create();
        var filmRepository = setup.FilmRepository;

        var NonExistingFilm = new Film { Id = 96, Name = "This Film Does not Exist" };

        filmRepository.Remove(NonExistingFilm);

        // Act & Assert
        await FluentActions
            .Awaiting(async () => await filmRepository.SaveChangesAsync())
            .Should()
            .ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SaveChangesAsync_Should_SaveChanges_Successfully()
    {
        // Arrange
        using var setup = RepositoryTestSetup.Create();
        var filmRepository = setup.FilmRepository;

        var film = new Film { Name = "Test Film" };
        await filmRepository.AddAsync(film);

        // Act
        await filmRepository.SaveChangesAsync();

        // Assert
        var result = await filmRepository.GetAsync(film.Id);
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(film);
    }

    [Fact]
    public async Task BeginTransactionAsync__Should_SuccessfulCommitData_ToDb()
    {
        // Arrange
        using var setup = RepositoryTestSetup.Create();
        var filmRepository = setup.FilmRepository;

        var film = Fakers.Film.Generate();

        // Act
        using var transaction = await filmRepository.BeginTransactionAsync();

        await filmRepository.AddAsync(film);
        await filmRepository.SaveChangesAsync();
        await transaction.CommitAsync();

        // Assert
        var result = await filmRepository.GetAsync(film.Id);
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(film);
    }

    [Fact]
    public async Task BeginTransactionAsync__Should_RollbackData_WhenExceptionWasThrow()
    {
        // Arrange
        using var setup = RepositoryTestSetup.Create();
        var filmRepository = setup.FilmRepository;

        var film = Fakers.Film.Generate();

        // Act
        using var transaction = await filmRepository.BeginTransactionAsync();

        await FluentActions
          .Awaiting(async () =>
          {
              await filmRepository.AddAsync(film);
              await filmRepository.SaveChangesAsync();
              throw new InvalidOperationException("Test exception");
          })
          .Should()
          .ThrowAsync<InvalidOperationException>();

        await transaction.RollbackAsync();

        // Assert
        var result = await filmRepository.GetList().ToListAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync__Should_ReturnListAsync()
    {
        // Arrange
        using var setup = RepositoryTestSetup.Create();
        var filmRepository = setup.FilmRepository;

        var films = await Fakers.Film.GenerateAndSaveAsync(3, setup.Context);

        // Act
        var result = await filmRepository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(films);
    }

    [Fact]
    public async Task FirstOrDefaultAsync__Should_ReturnFirstFilm_MatchingPredicate()
    {
        // Arrange
        using var setup = RepositoryTestSetup.Create();
        var filmRepository = setup.FilmRepository;

        var films = await Fakers.Film.GenerateAndSaveAsync(3, setup.Context);
        var targetFilm = films[1];

        // Act
        var result = await filmRepository.FirstOrDefaultAsync(f => f.Id == targetFilm.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(targetFilm);
    }


    [Fact]
    public async Task CountAsync__Should_ReturnFilmCount()
    {
        // Arrange
        using var setup = RepositoryTestSetup.Create();
        var filmRepository = setup.FilmRepository;

        var films = await Fakers.Film.GenerateAndSaveAsync(3, setup.Context);

        // Act
        var result = await filmRepository.CountAsync();
        // Assert
        result.Should().Be(3);
    }

}


