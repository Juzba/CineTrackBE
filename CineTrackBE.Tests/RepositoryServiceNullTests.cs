using CineTrackBE.AppServices;
using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace CineTrackBE.Tests;

public class RepositoryServiceNullTests
{
    private Repository<Film> _filmRepository;
    private Mock<ILogger<Repository<Film>>> _loggerMock;


    [SetUp]
    public void Setup()
    {


        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        var context = new ApplicationDbContext(options);

        _loggerMock = new Mock<ILogger<Repository<Film>>>();

        _filmRepository = new Repository<Film>(context, _loggerMock.Object);
    }




    [Test]
    public void AddAsync_NullEntity_ThrowsArgumentNullException()
    {

        Film nullFilm = null!;


        var exception = Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _filmRepository.AddAsync(nullFilm));


        Assert.That(exception.ParamName, Is.EqualTo("entity"));
    }


    [Test]
    public void GetAsync_Id_NullOrEmptyStringId_ThrowsArgumentNullException()
    {
        // Arrange
        string nullId = null!;
        string emptyId = "";

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _filmRepository.GetAsync_Id(nullId));
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _filmRepository.GetAsync_Id(emptyId));

        Assert.That(exception.ParamName, Is.EqualTo("id"));
    }


    [Test]
    public void Update_NullEntity_ThrowsArgumentNullException()
    {
        Film nullFilm = null!;


        var exception = Assert.Throws<ArgumentNullException>(() =>
                 _filmRepository.Update(nullFilm));


        Assert.That(exception.ParamName, Is.EqualTo("entity"));
    }


    [Test]
    public void Remove_NullEntity_ThrowsArgumentNullException()
    {
        Film nullFilm = null!;


        var exception = Assert.Throws<ArgumentNullException>(() =>
                 _filmRepository.Remove(nullFilm));


        Assert.That(exception.ParamName, Is.EqualTo("entity"));
    }


    [Test]
    public void AnyExistsAsync_NullOrEmptyStringId_ThrowsArgumentNullException()
    {
        // Arrange
        string nullId = null!;
        string emptyId = "";

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _filmRepository.AnyExistsAsync(nullId));
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _filmRepository.AnyExistsAsync(emptyId));

        Assert.That(exception.ParamName, Is.EqualTo("id"));
    }

}





















