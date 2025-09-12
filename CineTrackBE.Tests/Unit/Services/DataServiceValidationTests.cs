using CineTrackBE.AppServices;
using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using CineTrackBE.Tests.Helpers.TestSetups.Universal;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CineTrackBE.Tests.Unit.Services
{
    public class DataServiceValidationTests
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DataService> _logger;
        private readonly DataService _dataService;

        public DataServiceValidationTests()
        {
            // EF - IN MEMORY DB //
            _context = InMemoryDbTestSetup.Create().Context;

            _logger = new Mock<ILogger<DataService>>().Object;
            _dataService = new DataService(_context, _logger);
        }


        [Fact]
        public async Task AddUserRoleAsync_ThrowsArgumentNullException_WhenUserIsNull()
        {

            //   Act & Assert
            await FluentActions
                .Invoking(async () => await _dataService.AddUserRoleAsync(null!, "Admin"))
                .Should()
                .ThrowAsync<ArgumentNullException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task AddUserRoleAsync_ThrowsArgumentException_WhenRoleIsNullOrEmpty(string? role)
        {

            //   Act & Assert
            await FluentActions
                .Invoking(async () => await _dataService.AddUserRoleAsync(new(), role!))
                .Should()
                .ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task RemoveUserRoleAsync_ThrowsArgumentNullException_WhenUserIsNull()
        {

            //   Act & Assert
            await FluentActions
                .Invoking(async () => await _dataService.RemoveUserRoleAsync(null!, "Admin"))
                .Should()
                .ThrowAsync<ArgumentNullException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task RemoveUserRoleAsync_ThrowsArgumentException_WhenRoleIsNullOrEmpty(string? role)
        {

            //   Act & Assert
            await FluentActions
                .Invoking(async () => await _dataService.RemoveUserRoleAsync(new(), role!))
                .Should()
                .ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GetRolesFromUserAsync__ThrowsArgumentNullException_WhenUserIsNull()
        {

            //   Act & Assert
            await FluentActions
                .Invoking(async () => await _dataService.GetRolesFromUserAsync(null!))
                .Should()
                .ThrowAsync<ArgumentNullException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task CountUserInRoleAsync__ThrowsArgumentException_WhenRoleIsNullOrEmpty(string? role)
        {
            await FluentActions
                .Invoking(async () => await _dataService.CountUserInRoleAsync(role!))
                .Should()
                .ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task AddGenresToFilmAsync__ThrowsArgumentNullException_WhenFilmIsNull()
        {
            var genreIds = new List<int> { 1, 2, 3 };

            await FluentActions
                .Invoking(async () => await _dataService.AddGenresToFilmAsync(null!, genreIds))
                .Should()
                .ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task AddGenresToFilmAsync_ThrowsArgumentNullException_WhenGenreIdsIsNull()
        {
            var film = new Film { Id = 1, Name = "Test Film" };

            await FluentActions
                .Invoking(async () => await _dataService.AddGenresToFilmAsync(film, null!))
                .Should()
                .ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task AddGenresToFilmAsync_ThrowsArgumentException_WhenGenreIdsIsEmpty()
        {
            var film = new Film { Id = 1, Name = "Test Film" };

            await FluentActions
                .Invoking(async () => await _dataService.AddGenresToFilmAsync(film, []))
                .Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("The genreIds list is empty.*");
        }

        


    }
}
