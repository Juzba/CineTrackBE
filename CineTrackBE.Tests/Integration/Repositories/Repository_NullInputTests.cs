using CineTrackBE.AppServices;
using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CineTrackBE.Tests.Integration.Repositories
{
    public class Repository_NullInputTests
    {

        private readonly IRepository<Film> _repository;
        private readonly ApplicationDbContext _context;

        public Repository_NullInputTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
               .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
               .Options;

            _context = new ApplicationDbContext(options);
            var logger = new Mock<ILogger<Repository<Film>>>().Object;
            _repository = new Repository<Film>(_context, logger);
        }


        [Fact]
        public async Task AddAsync__ArgumentException_When_NullInput()
        {

            // Act & Assert
            await FluentActions
                .Invoking(async () => await _repository.AddAsync(null!))
                .Should()
                .ThrowAsync<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'entity')");
        }

        [Fact]
        public async Task AddRangeAsync__ArgumentException_When_NullInput()
        {

            await FluentActions
                .Invoking(async () => await _repository.AddRangeAsync(null!))
                .Should()
                .ThrowAsync<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'entities')");
        }

        [Fact]
        public async Task AddRangeAsync__ThrowException_When_InputIsEmpty()
        {
            await FluentActions
                .Invoking(async () => await _repository
                .AddRangeAsync([]))                     
                .Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("The collection is empty. (Parameter 'entities')");
        }


        [Fact]
        public async Task GetAsync_Id__ThrowException_When_NullInput()
        {
            await FluentActions
                .Invoking(async () => await _repository.GetAsync(null!))
                .Should()
                .ThrowAsync<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'id')");
        }


        [Fact]
        public void Update__ThrowException_When_NullInput()
        {
            FluentActions
                .Invoking(() => _repository.Update(null!))
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'entity')");
        }


        [Fact]
        public void Remove__ThrowException_When_NullInput()
        {
            FluentActions
                .Invoking(() => _repository.Remove(null!))
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'entity')");
        }


        [Fact]
        public void RemoveRange__ThrowException_When_NullInput()
        {
            FluentActions
                .Invoking(() => _repository.RemoveRange(null!))
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'entities')");
        }


        [Fact]
        public void RemoveRange__ThrowException_When_EmptyInput()
        {
            FluentActions
                .Invoking(() => _repository.RemoveRange([]))
                .Should()
                .Throw<ArgumentException>()
                .WithMessage("The collection is empty. (Parameter 'entities')");
        }



        [Fact]
        public async Task AnyExistsAsync__ThrowException_When_NullInput()
        {
            await FluentActions
                .Invoking(async () => await _repository.AnyExistsAsync(null!))
                .Should()
                .ThrowAsync<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'id')");
        }
    }
}
