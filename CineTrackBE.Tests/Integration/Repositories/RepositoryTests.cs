using CineTrackBE.Models.Entities;
using CineTrackBE.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CineTrackBE.Tests.Integration.Repositories;

public class RepositoryTests
{
    [Fact]
    public async Task AddAsync__Should_AddFilm_ToDb()
    {
        // Arrange
        using var setup = DatabaseTestHelper.CreateTestSetup();
        var filmRepository = setup.FilmRepository;

        var film = TestDataBuilder.FilmBuilder.Create().WithRandomData().Build();

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
        using var setup = DatabaseTestHelper.CreateTestSetup();
        var filmRepository = setup.FilmRepository;

        var films = TestDataBuilder.FilmBuilder.Create().ListWithRandomData(3).BuildList();

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
        using var setup = DatabaseTestHelper.CreateTestSetup();
        var filmRepository = setup.FilmRepository;

        var film = await TestDataBuilder.FilmBuilder.Create().WithRandomData().BuildAndSaveAsync(setup.Context);

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
        using var setup = DatabaseTestHelper.CreateTestSetup();
        var filmRepository = setup.FilmRepository;

        const int nonExistedId = 999;

        // Act
        var result = await filmRepository.GetAsync(nonExistedId);

        // Assert
        result.Should().BeNull("Non-existent film should return null");
    }


    [Fact]
    public async Task GetAsync__StringId_Should_GetRole_FromDb()
    {
        // Arrange
        using var setup = DatabaseTestHelper.CreateTestSetup();
        var roleRepository = setup.RoleRepository;

        var role = await TestDataBuilder.RoleBuilder.Create().WithRandomData().BuildAndSaveAsync(setup.Context);

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
        using var setup = DatabaseTestHelper.CreateTestSetup();
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
        using var setup = DatabaseTestHelper.CreateTestSetup();
        var filmRepository = setup.FilmRepository;

        // Create and save initial film
        var originalFilm = await TestDataBuilder.FilmBuilder.Create()
            .WithName("Test Before Update")
            .WithDirector("Test Director")
            .BuildAndSaveAsync(setup.Context);

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
        using var setup = DatabaseTestHelper.CreateTestSetup();
        var filmRepository = setup.FilmRepository;

        var nonExistentFilm = TestDataBuilder.FilmBuilder.Create().WithId(999).WithRandomData().Build();

        // Pre-condition check
        var exists = await filmRepository.AnyExistsAsync(nonExistentFilm.Id);
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
        using var setup = DatabaseTestHelper.CreateTestSetup();
        var filmRepository = setup.FilmRepository;

        var film = await TestDataBuilder.FilmBuilder.Create().WithRandomData().BuildAndSaveAsync(setup.Context);

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
        using var setup = DatabaseTestHelper.CreateTestSetup();
        var filmRepository = setup.FilmRepository;

        var nonExistedFilm = TestDataBuilder.FilmBuilder.Create().WithId(9999).WithRandomData().Build();

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
        using var setup = DatabaseTestHelper.CreateTestSetup();
        var filmRepository = setup.FilmRepository;

        var films = await TestDataBuilder.FilmBuilder.Create().ListWithRandomData(4).BuildListAndSaveAsync(setup.Context);


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
        using var setup = DatabaseTestHelper.CreateTestSetup();
        var filmRepository = setup.FilmRepository;

        // Arrange
        List<Film> NonExistedFilms =
            [
                new Film { Id = 98, Name = "NonExistingFilm1" },
                new Film { Id = 99, Name = "NonExistedFilm2" }
            ];

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
        using var setup = DatabaseTestHelper.CreateTestSetup();
        var filmRepository = setup.FilmRepository;

        var films = await TestDataBuilder.FilmBuilder.Create().ListWithRandomData(3).BuildListAndSaveAsync(setup.Context);

        // Act & Assert
        var result = filmRepository.GetList();
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IQueryable<Film>>();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task AnyExistsAsync__StringId_Should_ReturnTrue_WhenRoleExists()
    {
        // Arrange
        using var setup = DatabaseTestHelper.CreateTestSetup();
        var roleRepository = setup.RoleRepository;

        var role = new IdentityRole { Name = "User" };

        await roleRepository.AddAsync(role);
        await roleRepository.SaveChangesAsync();

        // Act
        var result = await roleRepository.AnyExistsAsync(role.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AnyExistsAsync__StringId_Should_ReturnFalse_WhenFilmDoesNotExists()
    {
        // Arrange
        using var setup = DatabaseTestHelper.CreateTestSetup();
        var roleRepository = setup.RoleRepository;

        const string NonExistingId = "This Id Does Not Exist";

        // Act
        var result = await roleRepository.AnyExistsAsync(NonExistingId);

        // Assert
        result.Should().BeFalse();
    }


    [Fact]
    public async Task AnyExistsAsync__IntId_Should_ReturnTrue_WhenFilmExists()
    {
        // Arrange
        using var setup = DatabaseTestHelper.CreateTestSetup();
        var filmRepository = setup.FilmRepository;

        var film = new Film { Name = "Existence Test Film" };

        await filmRepository.AddAsync(film);
        await filmRepository.SaveChangesAsync();

        // Act
        var result = await filmRepository.AnyExistsAsync(film.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AnyExistsAsync__IntId_Should_ReturnFalse_WhenFilmDoesNotExists()
    {
        // Arrange
        using var setup = DatabaseTestHelper.CreateTestSetup();
        var filmRepository = setup.FilmRepository;

        const int NonExistingId = 99999;

        // Act
        var result = await filmRepository.AnyExistsAsync(NonExistingId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SaveChangesAsync__Should_ThrowException_WhenDbUpdateExceptionOccurs()
    {
        // Arrange
        using var setup = DatabaseTestHelper.CreateTestSetup();
        var filmRepository = setup.FilmRepository;

        var NonExistingFilm = new Film { Id = 999, Name = "This Film Does not Exist" };

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
        using var setup = DatabaseTestHelper.CreateTestSetup();
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
        using var setup = DatabaseTestHelper.CreateTestSetup();
        var filmRepository = setup.FilmRepository;

        var film = TestDataBuilder.FilmBuilder.Create().WithRandomData().Build();

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
        using var setup = DatabaseTestHelper.CreateTestSetup();
        var filmRepository = setup.FilmRepository;

        var film = TestDataBuilder.FilmBuilder.Create().WithRandomData().Build();

        // Act
        using var transaction = await filmRepository.BeginTransactionAsync();
        try
        {
            await filmRepository.AddAsync(film);
            await filmRepository.SaveChangesAsync();

            throw new InvalidOperationException("Test exception");
        }
        catch (Exception)
        {
           await transaction.RollbackAsync();
           
        }

        // Assert
        var result = await filmRepository.GetList().ToListAsync();
        result.Should().BeEmpty();
    }
}


