using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using CineTrackBE.Services;
using Moq;
using System;

namespace CineTrackBE.Tests
{
    public class RepositoryServiceTests
    {
        private Mock<IRepository<Film>> _mockRepository;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new Mock<IRepository<Film>>();
        }




        [Test]
        public async Task AddAsync_ShouldCallRepositoryMethod()
        {
            // Arrange
            var film = new Film { Id = 1, Name = "Test Film" };

            // Act
            await _mockRepository.Object.AddAsync(film);

            // Assert
            _mockRepository.Verify(r => r.AddAsync(film, default), Times.Once );
        }

        [Test]
        public async Task GetAsync_Id_ShouldReturnCorrectFilm()
        {
            // Arrange
            var expectedFilm = new Film { Id = 1, Name = "Test Film" };
            _mockRepository.Setup(r => r.GetAsync_Id(1, default)).ReturnsAsync(expectedFilm);

            // Act
            var result = await _mockRepository.Object.GetAsync_Id(1);

            // Assert
            Assert.That(result, Is.EqualTo(expectedFilm));
        }

        [Test]
        public void Update_ShouldCallRepositoryMethod()
        {
            // Arrange
            var film = new Film { Id = 1, Name = "Updated Film" };

            // Act
            _mockRepository.Object.Update(film);

            // Assert
            _mockRepository.Verify(r => r.Update(film), Times.Once);
        }

        [Test]
        public void Remove_ShouldCallRepositoryMethod()
        {
            // Arrange
            var film = new Film { Id = 1, Name = "Film to Remove" };

            // Act
            _mockRepository.Object.Remove(film);

            // Assert
            _mockRepository.Verify(r => r.Remove(film), Times.Once);
        }

        [Test]
        public void GetList_ShouldReturnQueryableList()
        {
            // Arrange
            var films = new List<Film>
            {
                new Film { Id = 1, Name = "Film 1" },
                new Film { Id = 2, Name = "Film 2" }
            }.AsQueryable();

            _mockRepository.Setup(r => r.GetList()).Returns(films);

            // Act
            var result = _mockRepository.Object.GetList();

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result, Is.EquivalentTo(films));
        }

        [Test]
        public async Task AnyExistsAsync_ShouldReturnTrueForExistingId()
        {
            // Arrange
            _mockRepository.Setup(r => r.AnyExistsAsync(1, default)).ReturnsAsync(true);

            // Act
            var result = await _mockRepository.Object.AnyExistsAsync(1);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task SaveChangesAsync_ShouldCallRepositoryMethod()
        {
            // Act
            await _mockRepository.Object.SaveChangesAsync();

            // Assert
            _mockRepository.Verify(r => r.SaveChangesAsync(default), Times.Once);
        }

    }
}
