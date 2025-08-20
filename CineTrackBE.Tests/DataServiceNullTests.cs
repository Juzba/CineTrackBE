using CineTrackBE.AppServices;
using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CineTrackBE.Tests;

public class DataServiceNullTests
{

    private DataService _dataService;

    [SetUp]
    public void SetUp()
    {

        var option = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDB")
            .Options;

        var context = new ApplicationDbContext(option);

        _dataService = new DataService(context, NullLogger<DataService>.Instance);

    }


    [Test]
    public void AnyUserExistsByUserNameAsync_NullOrEmptyUserName_ThrowsArgumentException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() => _dataService.AnyUserExistsByUserNameAsync(null!));
        var exception = Assert.ThrowsAsync<ArgumentException>(() => _dataService.AnyUserExistsByUserNameAsync(""));

        Assert.That(exception.ParamName, Is.EqualTo("userName"));
    }


    [Test]
    public void AddUserRoleAsync_NullUser_ThrowsArgumentNullException()
    {
        // Arrange
        var exceptionUser = Assert.ThrowsAsync<ArgumentNullException>(() => _dataService.AddUserRoleAsync(null!, "SomeRole"));

        // Assert
        Assert.That(exceptionUser.ParamName, Is.EqualTo("user"));
    }


    [Test]
    public void AddUserRoleAsync_NullOrEmptyRole_ThrowsArgumentException()
    {
        // Arrange
        var user = new ApplicationUser();

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentNullException>(() => _dataService.AddUserRoleAsync(user, null!));
        Assert.ThrowsAsync<ArgumentException>(() => _dataService.AddUserRoleAsync(user, " "));

        Assert.That(exception.ParamName, Is.EqualTo("role"));
    }

    [Test]
    public void RemoveUserRoleAsync_NullUser_ThrowsArgumentNullException()
    {
        var exception = Assert.ThrowsAsync<ArgumentNullException>(() => _dataService.RemoveUserRoleAsync(null!, "SomeRole"));

        Assert.That(exception.ParamName, Is.EqualTo("user"));
    }

    [Test]
    public void RemoveUserRoleAsync_NullOrEmptyRole_ThrowsArgumentException()
    {
        var user = new ApplicationUser();

        var exception = Assert.ThrowsAsync<ArgumentNullException>(() => _dataService.RemoveUserRoleAsync(user, null!));
        Assert.ThrowsAsync<ArgumentException>(() => _dataService.RemoveUserRoleAsync(user, ""));

        Assert.That(exception.ParamName, Is.EqualTo("role"));
    }

    [Test]
    public void GetRolesFromUserAsync_NullUser_ThrowsArgumentNullException()
    {
        var exception = Assert.ThrowsAsync<ArgumentNullException>(() => _dataService.GetRolesFromUserAsync(null!));

        Assert.That(exception.ParamName, Is.EqualTo("user"));
    }

    [Test]
    public void CountUserInRoleAsync_NullOrEmptyRole_ThrowsArgumentException()
    {
        var exception = Assert.ThrowsAsync<ArgumentNullException>(() => _dataService.CountUserInRoleAsync(null!));
        Assert.ThrowsAsync<ArgumentException>(() => _dataService.CountUserInRoleAsync(""));

        Assert.That(exception.ParamName, Is.EqualTo("role"));
    }

    [Test]
    public void AddGenresToFilmAsync_NullFilm_ThrowsArgumentNullException()
    {
        var exception = Assert.ThrowsAsync<ArgumentNullException>(() => _dataService.AddGenresToFilmAsync(null!, []));

        Assert.That(exception.ParamName, Is.EqualTo("film"));
    }

    [Test]
    public void AddGenresToFilmAsync_NullGenreIds_ThrowsArgumentNullException()
    {
        var film = new Film();

        var exception = Assert.ThrowsAsync<ArgumentNullException>(() => _dataService.AddGenresToFilmAsync(film, null!));

        Assert.That(exception.ParamName, Is.EqualTo("genreIds"));
    }

}