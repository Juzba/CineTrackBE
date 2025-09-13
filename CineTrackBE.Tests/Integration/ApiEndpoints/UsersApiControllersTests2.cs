using CineTrackBE.AppServices;
using CineTrackBE.Models.DTO;
using CineTrackBE.Tests.Helpers.Common;
using CineTrackBE.Tests.Helpers.TestSetups;
using CineTrackBE.Tests.Helpers.TestSetups.Universal;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
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

    [Fact]
    public async Task DeleteUser_Should_Return_BadRequest_When_TryingToDeleteLoggedUser()
    {
        // Arrange 
        using var setup = UsersApiControllerTestSetup.Create();
        var (loggedUser, _, _) = await HttpContextTestSetup.Create().WithUser().BuildAndSaveAsync(setup.Controller, setup.Context);

        // Act 
        var result = await setup.Controller.DeleteUser(loggedUser.Id);

        // Assert 
        result.Should().NotBeNull();
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("You cannot delete your own account while logged in!");
    }


    [Fact]
    public async Task DeleteUser_Should_Return_NotFound_When_UserNotExist()
    {
        // Arrange 
        using var setup = UsersApiControllerTestSetup.Create();
        HttpContextTestSetup.Create().Build(setup.Controller);

        var nonExistentUserId = Guid.NewGuid().ToString()[..8];

        // Act 
        var result = await setup.Controller.DeleteUser(nonExistentUserId);

        // Assert 
        result.Should().NotBeNull();
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be($"User with Id '{nonExistentUserId}' not exist!");
    }

    [Fact]
    public async Task DeleteUser_Should_Return_BadRequest_When_IdIsEmpty()
    {
        // Arrange 
        using var setup = UsersApiControllerTestSetup.Create();

        var emptyUserId = string.Empty;

        // Act 
        var result = await setup.Controller.DeleteUser(emptyUserId);

        // Assert 
        result.Should().NotBeNull();
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Invalid user ID. ID cannot be empty.");
    }

    [Fact]
    public async Task DeleteUser_Should_Return_Status500InternalServerError_When_DbErrorOccurs()
    {
        // Arrange 
        using var setup = UsersApiControllerTestSetup.Create();
        HttpContextTestSetup.Create().Build(setup.Controller);

        var user = await Fakers.User.GenerateOneAndSaveAsync(setup.Context);

        // Simulate DB error by disposing the context
        setup.Context.Dispose();
        // Act 
        var result = await setup.Controller.DeleteUser(user.Id);

        // Assert 
        result.Should().NotBeNull();
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
        objectResult.Value.Should().Be("Error occurred while deleting user");
    }

    // EDIT USER //
    [Fact]
    public async Task EditUser_Should_Return_OkResult_With_EditedUserDto()
    {
        // Arrange 
        using var setup = UsersApiControllerTestSetup.Create();

        var user = await Fakers.User.GenerateOneAndSaveAsync(setup.Context);
        var roles = await Fakers.Role.GenerateAndSaveAsync(2, setup.Context);
        var userRoles = new List<IdentityUserRole<string>>()
        {
            new() { UserId = user.Id, RoleId = roles[0].Id },
            new() { UserId = user.Id, RoleId = roles[1].Id }
        };

        await setup.UserRoleRepository.AddRangeAsync(userRoles);
        await setup.UserRoleRepository.SaveChangesAsync();

        var editUserDto = new UserDto { Id = user.Id, UserName = "editedUserName", Email = "editedUserEmail@example.com" };

        // Act 
        var result = await setup.Controller.EditUser(user.Id, editUserDto);

        // Assert 
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<UserDto>();
        okResult.Value.Should().BeEquivalentTo(editUserDto);

        // Check Db
        var userInDb = await setup.UserRepository.GetAsync(user.Id);
        userInDb.Should().NotBeNull();
        userInDb.UserName.Should().Be(editUserDto.UserName);
        userInDb.Email.Should().Be(editUserDto.Email);
    }

    [Fact]
    public async Task EditUser_WithNewRoles_ShouldReplaceExistingUserRoles()
    {
        // Arrange 
        using var setup = UsersApiControllerTestSetup.Create();

        var user = await Fakers.User.GenerateOneAndSaveAsync(setup.Context);
        var roles = await Fakers.Role.GenerateAndSaveAsync(3, setup.Context);
        var userRoles = new List<IdentityUserRole<string>>()
        {
            new() { UserId = user.Id, RoleId = roles[0].Id },
            new() { UserId = user.Id, RoleId = roles[1].Id }
        };

        await setup.UserRoleRepository.AddRangeAsync(userRoles);
        await setup.UserRoleRepository.SaveChangesAsync();

        var targetedRole = roles[2];

        var editUserDto = new UserDto
        {
            Id = user.Id,
            UserName = "editedUserName",
            Email = "editedUserEmail@example.com",
            Roles = [targetedRole.Name]
        };

        // Act
        var result = await setup.Controller.EditUser(editUserDto.Id, editUserDto);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var userDtoResult = okResult.Value.Should().BeOfType<UserDto>().Subject;

        userDtoResult.Should().NotBeNull();
        userDtoResult.Roles.Should().ContainSingle();
        userDtoResult.Roles.First().Should().Be(targetedRole.Name);

        // Check Db
        var userRolesInDb = await setup.UserRoleRepository.GetList().Where(p => p.UserId == user.Id).ToListAsync();
        userRolesInDb.Should().ContainSingle();
        userRolesInDb.First().RoleId.Should().Be(targetedRole.Id);
    }

    [Fact]
    public async Task EditUser_Should_Return_BadRequest_When_RoleIsInvalid()
    {
        // Arrange 
        using var setup = UsersApiControllerTestSetup.Create();
        var user = await Fakers.User.GenerateOneAndSaveAsync(setup.Context);
        var roles = await Fakers.Role.GenerateAndSaveAsync(2, setup.Context);
        var userRoles = new List<IdentityUserRole<string>>()
        {
            new() { UserId = user.Id, RoleId = roles[0].Id },
            new() { UserId = user.Id, RoleId = roles[1].Id }
        };
        await setup.UserRoleRepository.AddRangeAsync(userRoles);
        await setup.UserRoleRepository.SaveChangesAsync();

        var invalidRole = "InvalidRole - This RoleName is not exist in Db";

        var editUserDto = new UserDto
        {
            Id = user.Id,
            UserName = "editedUserName",
            Email = "editedUserEmail@example.com",
            Roles = [.. roles.Select(p => p.Name), invalidRole]
        };

        // Act
        var result = await setup.Controller.EditUser(editUserDto.Id, editUserDto);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be($"Role do not exist: {invalidRole}");
    }

    [Fact]
    public async Task EditUser_Should_Return_StatusConflict_When_EmailIsUsedByAnotherUser()
    {
        // Arrange 
        using var setup = UsersApiControllerTestSetup.Create();
        var user1 = await Fakers.User.GenerateOneAndSaveAsync(setup.Context);
        var user2 = await Fakers.User.GenerateOneAndSaveAsync(setup.Context);
        var editUserDto = new UserDto
        {
            Id = user1.Id,
            UserName = "editedUserName",
            Email = user2.Email, // email already used by user2
        };

        // Act
        var result = await setup.Controller.EditUser(editUserDto.Id, editUserDto);

        // Assert
        result.Should().NotBeNull();
        var objectResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        objectResult.Value.Should().Be($"User with this Email already Exist {user2.Email}");
    }

    [Fact]
    public async Task EditUser_Should_Return_NotFound_When_UserNotExist()
    {
        // Arrange 
        using var setup = UsersApiControllerTestSetup.Create();

        var nonExistentUserId = Guid.NewGuid().ToString()[..8];
        var editUserDto = new UserDto
        {
            Id = nonExistentUserId,
            UserName = "editedUserName",
            Email = "editedUserEmail@example.com"
        };

        // Act
        var result = await setup.Controller.EditUser(nonExistentUserId, editUserDto);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        badRequestResult.Value.Should().Be($"User with Id '{nonExistentUserId}' not found!");
    }

    [Fact]
    public async Task EditUser_Should_Return_Status500InternalServerError_When_DbErrorOccurs()
    {
        // Arrange 
        var repositoryRoleMock = new Mock<IRepository<IdentityRole>>();
        repositoryRoleMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Simulated database error"));


        using var setup = UsersApiControllerTestSetup.Create(roleRepository: repositoryRoleMock.Object);

        var user = await Fakers.User.GenerateOneAndSaveAsync(setup.Context);

        var editUserDto = new UserDto
        {
            Id = user.Id,
            UserName = "editedUserName",
            Email = "editedUserEmail@example.com"
        };

        // Act
        var result = await setup.Controller.EditUser(editUserDto.Id, editUserDto);

        // Assert
        result.Should().NotBeNull();
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        objectResult.Value.Should().Be("Error occurred while trying to update User in db");
    }


    [Fact]
    public async Task EditUser_Should_Return_BadRequest_When_ModelStateIsInvalid()
    {
        // Arrange 
        using var setup = UsersApiControllerTestSetup.Create();
        var user = await Fakers.User.GenerateOneAndSaveAsync(setup.Context);
        var editUserDto = new UserDto
        {
            Id = user.Id,
            UserName = "", // Invalid: UserName is required
            Email = "editedUserEmail@example.com"
        };

        setup.Controller.ModelState.AddModelError("UserName", "UserName is required");
        // Act
        var result = await setup.Controller.EditUser(editUserDto.Id, editUserDto);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value!.ToString().Should().StartWith("User is not valid");
    }

    [Fact]
    public async Task EditUser_Should_Return_BadRequest_When_PasswordIsInvalid()
    {
        // Arrange 
        using var setup = UsersApiControllerTestSetup.Create();
        var user = await Fakers.User.GenerateOneAndSaveAsync(setup.Context);
        var editUserDto = new UserDto
        {
            Id = user.Id,
            UserName = "editedUserName",
            Email = "editedUserEmail@example.com",
            NewPassword = "short"  // invalid password
        };

        // Act
        var result = await setup.Controller.EditUser(editUserDto.Id, editUserDto);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Password must be at least 6 characters long!");
    }
}