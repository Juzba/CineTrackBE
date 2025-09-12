using CineTrackBE.Tests.Helpers.Common;
using CineTrackBE.Tests.Helpers.TestSetups;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CineTrackBE.Tests.Integration.ApiEndpoints;

public class UsersApiControllersTests2
{
    // DELETE USER //
    [Fact]
    public async Task DeleteUser_Should_Return_OkResult_And_DeleteUserWithRelatedData()
    {
        // Arrange 
        using var setup = UsersApiControllerTestSetup.Create();
        HttpContextTestSetup.Create().Build(setup.Controller);

        var user = await Fakers.User.GenerateOneAndSaveAsync(setup.Context);
        var roles = await Fakers.Role.GenerateAndSaveAsync(2, setup.Context);

        var film = await Fakers.Film.GenerateOneAndSaveAsync(setup.Context);
        var comment = await Fakers.Comment.RuleFor(p => p.AutorId, user.Id).RuleFor(p => p.FilmId, film.Id).GenerateOneAndSaveAsync(setup.Context);
        await Fakers.Rating.RuleFor(p => p.CommentId, comment.Id).GenerateOneAndSaveAsync(setup.Context);
        

        var userRoles = new List<IdentityUserRole<string>>()
        {
            new() { UserId = user.Id, RoleId = roles[0].Id },
            new() { UserId = user.Id, RoleId = roles[1].Id }
        };
        await setup.UserRoleRepository.AddRangeAsync(userRoles);
        await setup.UserRoleRepository.SaveChangesAsync();

        // Act 
        var result = await setup.Controller.DeleteUser(user.Id);

        // Assert 
        result.Should().NotBeNull();
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be($"User with Id {user.Id} has been deleted successfully.");

        // Verify all related data is cascade deleted
        var userCountInDb = await setup.UserRepository.CountAsync();
        userCountInDb.Should().Be(0, "user should be deleted from database");

        var userRolesInDb = await setup.UserRoleRepository.CountAsync();
        userRolesInDb.Should().Be(0, "user roles should be cascade deleted");

        var userCommentCountInDb = await setup.Context.Comments.CountAsync();
        userCommentCountInDb.Should().Be(0, "user comments should be cascade deleted");

        var ratingsInDbCount = await setup.Context.Ratings.CountAsync();
        ratingsInDbCount.Should().Be(0, "ratings should be cascade deleted");
    }


    // db error
    // wrong id string
    // user not exist
    // user cant remove himself























}
