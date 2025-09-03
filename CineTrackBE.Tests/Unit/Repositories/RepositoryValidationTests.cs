using CineTrackBE.AppServices;
using CineTrackBE.Models.Entities;
using CineTrackBE.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace CineTrackBE.Tests.Unit.Repositories;

public class RepositoryValidationTests
{
    private readonly IRepository<Film> _repository;

    public RepositoryValidationTests()
    {
        // EF - IN MEMORY DB //
        var context = DatabaseTestHelper.CreateInMemoryContext();
        _repository = DatabaseTestHelper.CreateRepository<Film>(context);
    }

    [Fact]
    public async Task AddAsync_ThrowsArgumentNullException_WhenEntityIsNull()
    {
        // Act & Assert
        await FluentActions
            .Invoking(async () => await _repository.AddAsync(null!))
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task AddRangeAsync_ThrowsArgumentNullException_WhenEntitiesIsNull()
    {
        // Act & Assert
        await FluentActions
            .Invoking(async () => await _repository.AddRangeAsync(null!))
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task AddRangeAsync_ThrowsArgumentException_WhenEntitiesIsEmpty()
    {
        // Act & Assert
        await FluentActions
            .Invoking(async () => await _repository.AddRangeAsync([]))
            .Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("The collection is empty. (Parameter 'entities')");
    }

    [Fact]
    public async Task GetAsync_ThrowsArgumentNullException_WhenStringIdIsNull()
    {
        // Act & Assert
        await FluentActions
            .Invoking(async () => await _repository.GetAsync(null!))
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void Update_ThrowsArgumentNullException_WhenEntityIsNull()
    {
        // Act & Assert
        FluentActions
            .Invoking(() => _repository.Update(null!))
            .Should()
            .Throw<ArgumentNullException>();
    }

    [Fact]
    public void Remove_ThrowsArgumentNullException_WhenEntityIsNull()
    {
        // Act & Assert
        FluentActions
            .Invoking(() => _repository.Remove(null!))
            .Should()
            .Throw<ArgumentNullException>();
    }

    [Fact]
    public void RemoveRange_ThrowsArgumentNullException_WhenEntitiesIsNull()
    {
        // Act & Assert
        FluentActions
            .Invoking(() => _repository.RemoveRange(null!))
            .Should()
            .Throw<ArgumentNullException>();
    }

    [Fact]
    public void RemoveRange_ThrowsArgumentException_WhenEntitiesIsEmpty()
    {
        // Act & Assert
        FluentActions
            .Invoking(() => _repository.RemoveRange([]))
            .Should()
            .Throw<ArgumentException>();
    }

    [Fact]
    public async Task AnyExistsAsync_ThrowsArgumentNullException_WhenStringIdIsNull()
    {
        // Act & Assert
        await FluentActions
            .Invoking(async () => await _repository.AnyExistsAsync(null!))
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task FirstOrDefaultAsync_ThrowsArgumentNullException_WhenPredicateIsNull()
    {
        // Act & Assert
        await FluentActions
            .Invoking(async () => await _repository.FirstOrDefaultAsync(null!))
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }
}