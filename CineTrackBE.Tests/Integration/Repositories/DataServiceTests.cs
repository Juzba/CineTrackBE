using CineTrackBE.AppServices;
using CineTrackBE.Data;
using CineTrackBE.Tests.Helpers.Common;
using CineTrackBE.Tests.Helpers.TestSetups.Universal;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace CineTrackBE.Tests.Integration.Repositories
{
    public class DataServiceTests(ITestOutputHelper output)
    {
        private readonly ITestOutputHelper _output = output;


        private static DataService CreateNewDataService(ApplicationDbContext context)
        {
            return new DataService(context, new Mock<ILogger<DataService>>().Object);
        }


        [Fact]
        public async Task AddUserRoleAsync__Should_Success_And_AddUserRoleToDatabase()
        {
            // Arrange
            using var context = InMemoryDbTestSetup.Create().Context;

            var user = await Fakers.User.GenerateOneAndSaveAsync(context);
            var role = await Fakers.Role.GenerateOneAndSaveAsync(context);

            // Act
            await CreateNewDataService(context).AddUserRoleAsync(user, role.Name!);

            // Assert
            var result = await context.UserRoles.ToListAsync();

            result.Should().NotBeEmpty();
            result.Should().ContainSingle();
            result.First().RoleId.Should().Be(role.Id);
            result.First().UserId.Should().Be(user.Id);
        }

        // ADD USER ROLE //
        [Fact]
        public async Task AddUserRoleAsync__Should_ThrowArgumentException_When_RoleNotFoundInDatabase()
        {
            // Arrange
            using var context = InMemoryDbTestSetup.Create().Context;
            var user = await Fakers.User.GenerateOneAndSaveAsync(context);
            var roleName = "NonExistingRole";

            // Act
            Func<Task> act = async () => await CreateNewDataService(context).AddUserRoleAsync(user, roleName);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Role does not exist.*");
        }

        [Fact]
        public async Task AddUserRoleAsync__Should_ThrowArgumentException_When_UserAlreadyHaveRole()
        {
            // Arrange
            using var context = InMemoryDbTestSetup.Create().Context;

            var user = await Fakers.User.GenerateOneAndSaveAsync(context);
            var role = await Fakers.Role.GenerateOneAndSaveAsync(context);

            await CreateNewDataService(context).AddUserRoleAsync(user, role.Name!);

            // Act
            Func<Task> act = async () => await CreateNewDataService(context).AddUserRoleAsync(user, role.Name!);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("The user already has this role assigned.*");
        }

        // REMOVE USER ROLE //
        [Fact]
        public async Task RemoveUserRoleAsync__Should_Success_And_RemoveUserRoleFromDatabase()
        {
            // Arrange
            using var context = InMemoryDbTestSetup.Create().Context;
            var user = await Fakers.User.GenerateOneAndSaveAsync(context);
            var role = await Fakers.Role.GenerateOneAndSaveAsync(context);

            var userRole = new IdentityUserRole<string> { UserId = user.Id, RoleId = role.Id };
            await context.UserRoles.AddAsync(userRole);
            await context.SaveChangesAsync();

            var checkCount = await context.UserRoles.CountAsync();
            checkCount.Should().Be(1);

            // Act
            await CreateNewDataService(context).RemoveUserRoleAsync(user, role.Name!);
            // Assert
            var result = await context.UserRoles.ToListAsync();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task RemoveUserRoleAsync__Should_ThrowArgumentException_When_RoleNotFoundInDatabase()
        {
            // Arrange
            using var context = InMemoryDbTestSetup.Create().Context;
            var user = await Fakers.User.GenerateOneAndSaveAsync(context);
            var roleName = "NonExistingRole";

            // Act
            Func<Task> act = async () => await CreateNewDataService(context).RemoveUserRoleAsync(user, roleName);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Role does not exist.*");
        }

        [Fact]
        public async Task RemoveUserRoleAsync__Should_ThrowArgumentException_When_UserDoesNotHaveRoleAssigned()
        {
            // Arrange
            using var context = InMemoryDbTestSetup.Create().Context;
            var user = await Fakers.User.GenerateOneAndSaveAsync(context);
            var role = await Fakers.Role.GenerateOneAndSaveAsync(context);

            // Act
            Func<Task> act = async () => await CreateNewDataService(context).RemoveUserRoleAsync(user, role.Name!);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("The user does not have this role assigned.*");
        }


        // GET ROLES FROM USER //
        [Fact]
        public async Task GetRolesFromUserAsync__Should_ReturnRolesAssignedToUser()
        {
            // Arrange
            using var context = InMemoryDbTestSetup.Create().Context;
            var user = await Fakers.User.GenerateOneAndSaveAsync(context);
            var role1 = await Fakers.Role.GenerateOneAndSaveAsync(context);
            var role2 = await Fakers.Role.GenerateOneAndSaveAsync(context);

            var userRole1 = new IdentityUserRole<string> { UserId = user.Id, RoleId = role1.Id };
            var userRole2 = new IdentityUserRole<string> { UserId = user.Id, RoleId = role2.Id };

            await context.UserRoles.AddRangeAsync(userRole1, userRole2);
            await context.SaveChangesAsync();

            // Act
            var result = await CreateNewDataService(context).GetRolesFromUserAsync(user);

            // Assert
            result.Should().NotBeEmpty();
            result.Should().HaveCount(2);
            result.Should().Contain(r => r.Id == role1.Id && r.Name == role1.Name);
            result.Should().Contain(r => r.Id == role2.Id && r.Name == role2.Name);
        }


        // COUNT USER IN ROLE //
        [Fact]
        public async Task CountUserInRoleAsync__Should_ReturnCountOfUsersInRole()
        {
            // Arrange
            using var context = InMemoryDbTestSetup.Create().Context;

            var role = await Fakers.Role.GenerateOneAndSaveAsync(context);
            var user1 = await Fakers.User.GenerateOneAndSaveAsync(context);
            var user2 = await Fakers.User.GenerateOneAndSaveAsync(context);
            var user3 = await Fakers.User.GenerateOneAndSaveAsync(context);

            var userRole1 = new IdentityUserRole<string> { UserId = user1.Id, RoleId = role.Id };
            var userRole2 = new IdentityUserRole<string> { UserId = user2.Id, RoleId = role.Id };

            await context.UserRoles.AddRangeAsync(userRole1, userRole2);
            await context.SaveChangesAsync();

            // Act
            var result = await CreateNewDataService(context).CountUserInRoleAsync(role.Name!);

            // Assert
            result.Should().Be(2);
        }


        // ADD LIST OF GENRES TO FILM //
        [Fact]
        public async Task AddGenresToFilmAsync__Should_Success_And_AddGenresToFilm()
        {
            // Arrange
            using var context = InMemoryDbTestSetup.Create().Context;

            var film = await Fakers.Film.GenerateOneAndSaveAsync(context);
            var genre1 = await Fakers.Genre.GenerateOneAndSaveAsync(context);
            var genre2 = await Fakers.Genre.GenerateOneAndSaveAsync(context);
            var genreIds = new List<int> { genre1.Id, genre2.Id };

            // Act
            await CreateNewDataService(context).AddGenresToFilmAsync(film, genreIds);

            // Assert
            var result = await context.FilmGenres.Where(fg => fg.FilmId == film.Id).ToListAsync();
            result.Should().NotBeEmpty();
            result.Should().HaveCount(2);

            result.Should().Contain(fg => fg.GenreId == genre1.Id);
            result.Should().Contain(fg => fg.GenreId == genre2.Id);
        }
    }
}
