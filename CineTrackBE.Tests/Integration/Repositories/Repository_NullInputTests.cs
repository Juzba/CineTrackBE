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
        public async Task AddAsync__ArgumentExceptation_When_NullInput()
        {

            // Act & Assert
            await FluentActions
                .Invoking(async () => await _repository.AddAsync(null!))
                .Should()
                .ThrowAsync<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'entity')");
        }



    }
}
