using CineTrackBE.AppServices;
using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using CineTrackBE.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CineTrackBE.Tests.Integration.Repositories
{
    public class Repository_NullInputTests
    {

        [Fact]
        public async Task AddAsync__ArgumentException_When_NullInput()
        {
            // Arrange
            using var setup = DatabaseTestHelper.CreateSqlLiteTestSetup();
            var filmRepository = setup.FilmRepository;

            // Act & Assert
            await FluentActions
                .Invoking(async () => await filmRepository.AddAsync(null!))
                .Should()
                .ThrowAsync<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'entity')");
        }

        [Fact]
        public async Task AddRangeAsync__ArgumentException_When_NullInput()
        {
            // Arrange
            using var setup = DatabaseTestHelper.CreateSqlLiteTestSetup();
            var filmRepository = setup.FilmRepository;

            // Act & Assert
            await FluentActions
                .Invoking(async () => await filmRepository.AddRangeAsync(null!))
                .Should()
                .ThrowAsync<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'entities')");
        }

        [Fact]
        public async Task AddRangeAsync__ThrowException_When_InputIsEmpty()
        {
            // Arrange
            using var setup = DatabaseTestHelper.CreateSqlLiteTestSetup();
            var filmRepository = setup.FilmRepository;

            // Act & Assert
            await FluentActions
                .Invoking(async () => await filmRepository
                .AddRangeAsync([]))                     
                .Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("The collection is empty. (Parameter 'entities')");
        }


        [Fact]
        public async Task GetAsync_Id__ThrowException_When_NullInput()
        {
            // Arrange
            using var setup = DatabaseTestHelper.CreateSqlLiteTestSetup();
            var filmRepository = setup.FilmRepository;

            // Act & Assert
            await FluentActions
                .Invoking(async () => await filmRepository.GetAsync(null!))
                .Should()
                .ThrowAsync<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'id')");
        }


        [Fact]
        public void Update__ThrowException_When_NullInput()
        {
            // Arrange
            using var setup = DatabaseTestHelper.CreateSqlLiteTestSetup();
            var filmRepository = setup.FilmRepository;

            // Act & Assert
            FluentActions
                .Invoking(() => filmRepository.Update(null!))
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'entity')");
        }


        [Fact]
        public void Remove__ThrowException_When_NullInput()
        {
            // Arrange
            using var setup = DatabaseTestHelper.CreateSqlLiteTestSetup();
            var filmRepository = setup.FilmRepository;

            // Act & Assert
            FluentActions
                .Invoking(() => filmRepository.Remove(null!))
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'entity')");
        }


        [Fact]
        public void RemoveRange__ThrowException_When_NullInput()
        {
            // Arrange
            using var setup = DatabaseTestHelper.CreateSqlLiteTestSetup();
            var filmRepository = setup.FilmRepository;

            // Act & Assert
            FluentActions
                .Invoking(() => filmRepository.RemoveRange(null!))
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'entities')");
        }


        [Fact]
        public void RemoveRange__ThrowException_When_EmptyInput()
        {
            // Arrange
            using var setup = DatabaseTestHelper.CreateSqlLiteTestSetup();
            var filmRepository = setup.FilmRepository;

            // Act & Assert
            FluentActions
                .Invoking(() => filmRepository.RemoveRange([]))
                .Should()
                .Throw<ArgumentException>()
                .WithMessage("The collection is empty. (Parameter 'entities')");
        }



        [Fact]
        public async Task AnyExistsAsync__ThrowException_When_NullInput()
        {
            // Arrange
            using var setup = DatabaseTestHelper.CreateSqlLiteTestSetup();
            var filmRepository = setup.FilmRepository;

            // Act & Assert
            await FluentActions
                .Invoking(async () => await filmRepository.AnyExistsAsync(null!))
                .Should()
                .ThrowAsync<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'id')");
        }

        [Fact]
        public async Task FirstOrDefaultAsync_Should()
        {
            // Arrange
            using var setup = DatabaseTestHelper.CreateSqlLiteTestSetup();
            var filmRepository = setup.FilmRepository;

            // Act & Assert







        }


        /// Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)

    }
}
