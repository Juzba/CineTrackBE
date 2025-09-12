using CineTrackBE.AppServices;
using CineTrackBE.Models.DTO;
using CineTrackBE.Models.Entities;
using CineTrackBE.Tests.Helpers.Common;
using CineTrackBE.Tests.Helpers.TestSetups;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Xunit;

namespace CineTrackBE.Tests.Integration.ApiEndpoints;

public class UsersApiControllerTests1
{

    // GET ALL USERS //
    [Fact]
    public async Task GetAllUsers_Should_Return_OkStatus_WithEmpty_EnumerableUserDto()
    {
        // Arrange 
        using var setup = UsersApiControllerTestSetup.Create();

        // Act 
        var result = await setup.Controller.GetAllUsers();

        // Assert 
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeAssignableTo<IEnumerable<UserDto>>().Subject;
        returnValue.Should().NotBeNull();
        returnValue.Should().BeEmpty();
    }


    [Fact]
    public async Task GetAllUsers_Should_Return_EnumerableUserDto_IncludeRoles()
    {
        // Arrange 
        using var setup = UsersApiControllerTestSetup.Create();

        var users = await Fakers.User.GenerateAndSaveAsync(2, setup.Context);
        var roles = await Fakers.Role.GenerateAndSaveAsync(2, setup.Context);

        var user1 = users[0];
        var user2 = users[1];

        var userRoles = new List<IdentityUserRole<string>>()
        {
            new() { UserId = user1.Id, RoleId = roles[0].Id },
            new() { UserId = user1.Id, RoleId = roles[1].Id },
            new() { UserId = user2.Id, RoleId = roles[0].Id }
        };

        await setup.UserRoleRepository.AddRangeAsync(userRoles);
        await setup.UserRoleRepository.SaveChangesAsync();

        var user1RoleNames = roles
            .Where(r => userRoles.Where(ur => ur.UserId == user1.Id).Select(ur => ur.RoleId).Contains(r.Id))
            .Select(r => r.Name)
            .ToList();

        var user2RoleNames = roles
            .Where(r => userRoles.Where(ur => ur.UserId == user2.Id).Select(ur => ur.RoleId).Contains(r.Id))
            .Select(r => r.Name)
            .ToList();

        // Act 
        var result = await setup.Controller.GetAllUsers();

        // Assert 
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnUsersDto = okResult.Value.Should().BeAssignableTo<IEnumerable<UserDto>>().Subject;

        returnUsersDto.Should().NotBeEmpty();

        // Assert User1 include roles
        var returnedUser1 = returnUsersDto.First(p => p.Id == user1.Id);
        returnedUser1.UserName.Should().Be(user1.UserName);
        returnedUser1.PhoneNumber.Should().Be(user1.PhoneNumber);
        returnedUser1.Roles.Should().BeEquivalentTo(user1RoleNames);

        // Assert User2 include roles
        var returnedUser2 = returnUsersDto.First(p => p.Id == user2.Id);
        returnedUser2.UserName.Should().Be(user2.UserName);
        returnedUser2.PhoneNumber.Should().Be(user2.PhoneNumber);
        returnedUser2.Roles.Should().BeEquivalentTo(user2RoleNames);
    }

    [Fact]
    public async Task GetAllUsers_Should_Return_Status500InternalServerError_WhenDbErrorOccurs()
    {
        // Arrange 
        var userRepositoryMock = new Mock<IRepository<ApplicationUser>>();
        userRepositoryMock
            .Setup(repo => repo.GetList())
            .Throws(new Exception("Database error test -> userRepositoryMock > GetList()"));

        using var setup = UsersApiControllerTestSetup.Create(userRepository: userRepositoryMock.Object);

        // Act 
        var result = await setup.Controller.GetAllUsers();

        // Assert 
        result.Should().NotBeNull();
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        objectResult.Value.Should().Be("Error occurred while trying to retrieve Users from the database.");
    }


    // ADD USER //
    [Fact]
    public async Task AddUser_Should_Return_CreatedAtActionResult_WithUserDto()
    {
        // Arrange 
        using var setup = UsersApiControllerTestSetup.Create();

        var newUserDto = Fakers.UserDto.Generate();

        // Act 
        var result = await setup.Controller.AddUser(newUserDto);

        // Assert 
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var returnValue = okResult.Value.Should().BeAssignableTo<UserDto>().Subject;

        returnValue.Should().NotBeNull();
        returnValue.Id.Should().NotBeNullOrWhiteSpace();
        returnValue.UserName.Should().Be(newUserDto.UserName);
        returnValue.PhoneNumber.Should().Be(newUserDto.PhoneNumber);
        returnValue.Roles.Should().BeNull();
    }

    [Fact]
    public async Task AddUser_Should_AddUserToDb_IncludingRoles()
    {
        // Arrange 
        using var setup = UsersApiControllerTestSetup.Create();

        var roles = await Fakers.Role.GenerateAndSaveAsync(2, setup.Context);
        var roleNames = roles.Select(r => r.Name).ToList();
        var newUserDto = Fakers.UserDto.Generate();
        newUserDto.Roles = roleNames;

        // Act 
        var result = await setup.Controller.AddUser(newUserDto);

        // Assert 
        result.Should().NotBeNull();
        var createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var returnValue = createdAtActionResult.Value.Should().BeAssignableTo<UserDto>().Subject;

        // Verify user is added to db including roles
        var addedUser = await setup.Context.Users.FindAsync(returnValue.Id);

        addedUser.Should().NotBeNull();
        addedUser.UserName.Should().Be(newUserDto.UserName);
        addedUser.PhoneNumber.Should().Be(newUserDto.PhoneNumber);

        var userRolesInDb = setup.Context.UserRoles.Where(ur => ur.UserId == addedUser.Id).ToList();
        userRolesInDb.Should().HaveCount(roleNames.Count);
        foreach (var roleName in roleNames)
        {
            var role = roles.First(r => r.Name == roleName);
            userRolesInDb.Should().Contain(ur => ur.RoleId == role.Id && ur.UserId == addedUser.Id);
        }
    }

    [Fact]
    public async Task AddUser_Should_Return_Conflict_WhenEmailAlreadyExist()
    {
        // Arrange 
        using var setup = UsersApiControllerTestSetup.Create();

        var existingUser = await Fakers.User.GenerateOneAndSaveAsync(setup.Context);

        var newUserDto = Fakers.UserDto.Generate();
        newUserDto.Email = existingUser.Email; // Set email to existing user's email

        // Act 
        var result = await setup.Controller.AddUser(newUserDto);

        // Assert 
        result.Should().NotBeNull();
        var conflictResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        conflictResult.Value.Should().Be($"User with this Email already Exist {existingUser.Email}");
    }


    [Fact]
    public async Task AddUser_Should_Return_Status500_When_DbError_Occurs()
    {
        // Arrange
        var userRepositoryMock = new Mock<IRepository<ApplicationUser>>();

        userRepositoryMock.Setup(p => p.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mock<IDbContextTransaction>().Object);

        userRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error test -> userRepositoryMock > AddAsync()"));

        using var setup = UsersApiControllerTestSetup.Create(userRepository: userRepositoryMock.Object);

        var newUserDto = Fakers.UserDto.Generate();

        // Act
        var result = await setup.Controller.AddUser(newUserDto);

        // Assert
        result.Should().NotBeNull();
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        objectResult.Value.Should().Be("Error occurred while trying to save User to db");
    }

    [Fact]
    public async Task AddUser_Should_Return_BadRequest_When_ModelStateIsInvalid()
    {
        // Arrange
        using var setup = UsersApiControllerTestSetup.Create();

        setup.Controller.ModelState.AddModelError("Email", "Email is required");

        var newUserDto = Fakers.UserDto.Generate();

        // Act
        var result = await setup.Controller.AddUser(newUserDto);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().BeOfType<SerializableError>();
    }

    [Fact]
    public async Task AddUser_Should_Return_BadRequest_When_PasswordIsShort()
    {
        // Arrange
        using var setup = UsersApiControllerTestSetup.Create();

        var newUserDto = Fakers.UserDto.Generate();
        newUserDto.NewPassword = "short"; // Set invalid password

        // Act
        var result = await setup.Controller.AddUser(newUserDto);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Password must be at least 6 characters long!");
    }

    [Fact]
    public async Task AddUser_Should_Return_BadRequest_When_PasswordIsNull()
    {
        // Arrange
        using var setup = UsersApiControllerTestSetup.Create();

        var newUserDto = Fakers.UserDto.Generate();
        newUserDto.NewPassword = null; // Set invalid password

        // Act
        var result = await setup.Controller.AddUser(newUserDto);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Password must be at least 6 characters long!");
    }

    [Fact]
    public async Task AddUser_Should_Return_BadRequest_When_RoleDoesNotExist()
    {
        // Arrange
        using var setup = UsersApiControllerTestSetup.Create();

        var role = Fakers.Role.GenerateAndSaveAsync(2, setup.Context);

        var newUserDto = Fakers.UserDto.Generate();
        newUserDto.Roles = ["NonExistentRole"]; // Set invalid role

        // Act
        var result = await setup.Controller.AddUser(newUserDto);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value!.ToString().Should().StartWith("Roles do not exist:");
    }

    [Fact]
    public async Task AddUser_Should_RollbackTransaction_When_DbError_Occurs()
    {
        // Arrange
        var userRoleRepositoryMock = new Mock<IRepository<IdentityUserRole<string>>>();
        userRoleRepositoryMock
            .Setup(repo => repo.AddRangeAsync(It.IsAny<IEnumerable<IdentityUserRole<string>>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error test -> userRoleRepositoryMock > AddRangeAsync()"));

        using var setup = UsersApiControllerTestSetup.Create(userRoleRepository: userRoleRepositoryMock.Object);

        var role = await Fakers.Role.GenerateOneAndSaveAsync(setup.Context);
        var newUserDto = Fakers.UserDto.Generate();
        newUserDto.Roles = [role.Name];

        // Act
        var result = await setup.Controller.AddUser(newUserDto);

        // Assert
        result.Should().NotBeNull();
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        objectResult.Value.Should().Be("Error occurred while trying to save User to db");

        // check if user or role not be added
        var existUser = await setup.UserRepository.CountAsync();
        existUser.Should().Be(0);
        var existUserRoles = await setup.UserRoleRepository.CountAsync();
        existUserRoles.Should().Be(0);

        // Verify that rollback was called
        userRoleRepositoryMock.Verify(repo => repo.AddRangeAsync(It.IsAny<IEnumerable<IdentityUserRole<string>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

}
