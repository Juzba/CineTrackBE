using CineTrackBE.ApiControllers;
using CineTrackBE.Models.DTO;
using CineTrackBE.Tests.Helpers.Common;
using CineTrackBE.Tests.Helpers.TestSetups;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;
using static CineTrackBE.ApiControllers.AuthApiController;

namespace CineTrackBE.Tests.Integration.ApiEndpoints;

public class AuthApiControllerTests
{
    [Fact]
    public async Task LoginAsync__Should_Return_OkResult_WithAuthResponse()
    {
        // Arrange
        using var setup = AuthApiControllerTestSetup.Create();

        await setup.CreateUserAsync("test@example.com", "TestPassword123!", "Admin", "User");
        //Fakers.User

        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "TestPassword123!"
        };

        // Act
        var result = await setup.Controller.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var authResponse = okResult.Value.Should().BeOfType<AuthResponseDto>().Subject;

        authResponse.Should().NotBeNull();
        authResponse.Token.Should().NotBeNullOrEmpty();
        authResponse.User.Should().NotBeNull();
        authResponse.User.Email.Should().Be("test@example.com");
        authResponse.User.UserName.Should().Be("test@example.com");
        authResponse.User.Roles.Should().Contain("Admin");
        authResponse.User.Roles.Should().Contain("User");
    }

    [Fact]
    public async Task LoginAsync__Should_Generate_ValidJwtToken()
    {
        // Arrange
        using var setup = AuthApiControllerTestSetup.Create();

        await setup.CreateUserAsync("test@example.com", "TestPassword123!", "Admin");

        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "TestPassword123!"
        };

        // Act
        var result = await setup.Controller.LoginAsync(loginDto);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var authResponse = okResult.Value.Should().BeOfType<AuthResponseDto>().Subject;

        var tokenHandler = new JwtSecurityTokenHandler();
        tokenHandler.CanReadToken(authResponse.Token).Should().BeTrue();

        var jwtToken = tokenHandler.ReadJwtToken(authResponse.Token);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "test@example.com");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public async Task LoginAsync__Should_Return_Unauthorized_When_UserNotFound()
    {
        // Arrange
        using var setup = AuthApiControllerTestSetup.Create();

        var loginDto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "TestPassword123!"
        };

        // Act
        var result = await setup.Controller.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedResult>().Subject;
    }

    [Fact]
    public async Task LoginAsync__Should_Return_Unauthorized_When_InvalidPassword()
    {
        // Arrange
        using var setup = AuthApiControllerTestSetup.Create();

        await setup.CreateUserAsync("test@example.com", "CorrectPassword123!");

        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "WrongPassword123!"
        };

        // Act
        var result = await setup.Controller.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedResult>().Subject;
    }

    [Fact]
    public async Task LoginAsync__Should_Return_BadRequest_When_ModelState_IsInvalid()
    {
        // Arrange
        using var setup = AuthApiControllerTestSetup.Create();

        var loginDto = new LoginDto
        {
            Email = "invalid-email", // Invalid email format
            Password = "TestPassword123!"
        };

        setup.Controller.ModelState.AddModelError("Email", "Invalid email format");

        // Act
        var result = await setup.Controller.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
    }

    [Fact]
    public async Task LoginAsync__Should_Return_OkResult_When_UserHasNoRoles()
    {
        // Arrange
        using var setup = AuthApiControllerTestSetup.Create();

        await setup.CreateUserAsync("test@example.com", "TestPassword123!"); // No roles

        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "TestPassword123!"
        };

        // Act
        var result = await setup.Controller.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var authResponse = okResult.Value.Should().BeOfType<AuthResponseDto>().Subject;

        authResponse.Token.Should().NotBeNullOrEmpty();
        authResponse.User.Roles.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task LoginAsync__Should_Return_BadRequest_When_Email_IsNullOrEmpty(string? email)
    {
        // Arrange
        using var setup = AuthApiControllerTestSetup.Create();

        var loginDto = new LoginDto
        {
            Email = email,
            Password = "TestPassword123!"
        };

        setup.Controller.ModelState.AddModelError("Email", "Required");

        // Act
        var result = await setup.Controller.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task LoginAsync__Should_Return_BadRequest_When_Password_IsNullOrEmpty(string? password)
    {
        // Arrange
        using var setup = AuthApiControllerTestSetup.Create();

        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = password
        };

        setup.Controller.ModelState.AddModelError("Password", "Required");

        // Act
        var result = await setup.Controller.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
    }


    [Fact]
    public async Task RegisterAsync__Should_Return_CreatedAtActionResult_When_ValidData()
    {
        // Arrange
        using var setup = AuthApiControllerTestSetup.Create();

        var registerDto = new RegisterDto
        {
            Email = "newuser@example.com",
            Password = "TestPassword123!"
        };

        // Act
        var result = await setup.Controller.RegisterAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        var createdAtActionResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdAtActionResult.ActionName.Should().Be(nameof(AuthApiController.LoginAsync));
        createdAtActionResult.RouteValues.Should().ContainKey("username");
        createdAtActionResult.RouteValues["username"].Should().Be("newuser@example.com");
    }

    [Fact]
    public async Task RegisterAsync__Should_CreateUser_InDatabase()
    {
        // Arrange
        using var setup = AuthApiControllerTestSetup.Create();

        var registerDto = new RegisterDto
        {
            Email = "newuser@example.com",
            Password = "TestPassword123!"
        };

        // Act
        await setup.Controller.RegisterAsync(registerDto);

        // Assert
        var user = await setup.UserManager.FindByEmailAsync("newuser@example.com");
        user.Should().NotBeNull();
        user!.Email.Should().Be("newuser@example.com");
        user.UserName.Should().Be("newuser@example.com");

        var usersInDb = await setup.Context.Users.ToListAsync();
        usersInDb.Should().HaveCount(1);
    }

    [Fact]
    public async Task RegisterAsync__Should_Return_Conflict_When_UserAlreadyExists()
    {
        // Arrange
        using var setup = AuthApiControllerTestSetup.Create();

        // Create existing user
        await setup.CreateUserAsync("existing@example.com", "ExistingPassword123!");

        var registerDto = new RegisterDto
        {
            Email = "existing@example.com",
            Password = "NewPassword123!"
        };

        // Act
        var result = await setup.Controller.RegisterAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.Value.Should().Be("User with this Email already exists");
    }

    [Fact]
    public async Task RegisterAsync__Should_Return_BadRequest_When_ModelState_IsInvalid()
    {
        // Arrange
        using var setup = AuthApiControllerTestSetup.Create();

        var registerDto = new RegisterDto
        {
            Email = "invalid-email", // Invalid email format
            Password = "TestPassword123!"
        };

        setup.Controller.ModelState.AddModelError("Email", "Invalid email format");

        // Act
        var result = await setup.Controller.RegisterAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
    }

    [Fact]
    public async Task RegisterAsync__Should_Return_BadRequest_When_Password_TooWeak()
    {
        // Arrange
        using var setup = AuthApiControllerTestSetup.Create();

        // Update password requirements for this test
        var userManager = setup.UserManager;
        var options = userManager.Options.Password;
        options.RequireDigit = true;
        options.RequiredLength = 8;
        options.RequireNonAlphanumeric = true;
        options.RequireUppercase = true;
        options.RequireLowercase = true;

        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "weak" // Too weak password
        };

        // Act
        var result = await setup.Controller.RegisterAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task RegisterAsync__Should_Not_CreateUser_When_RegistrationFails()
    {
        // Arrange
        using var setup = AuthApiControllerTestSetup.Create();

        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "" // Empty password should fail
        };

        // Act
        await setup.Controller.RegisterAsync(registerDto);

        // Assert
        var user = await setup.UserManager.FindByEmailAsync("test@example.com");
        user.Should().BeNull();

        var usersInDb = await setup.Context.Users.ToListAsync();
        usersInDb.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task RegisterAsync__Should_Return_BadRequest_When_Email_IsNullOrEmpty(string? email)
    {
        // Arrange
        using var setup = AuthApiControllerTestSetup.Create();

        var registerDto = new RegisterDto
        {
            Email = email!,
            Password = "TestPassword123!"
        };

        setup.Controller.ModelState.AddModelError("Email", "Required");

        // Act
        var result = await setup.Controller.RegisterAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task RegisterAsync__Should_Return_BadRequest_When_Password_IsNullOrEmpty(string? password)
    {
        // Arrange
        using var setup = AuthApiControllerTestSetup.Create();

        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = password!
        };

        setup.Controller.ModelState.AddModelError("Password", "Required");

        // Act
        var result = await setup.Controller.RegisterAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test")]
    public async Task RegisterAsync__Should_Return_BadRequest_When_Email_FormatIsInvalid(string invalidEmail)
    {
        // Arrange
        using var setup = AuthApiControllerTestSetup.Create();

        var registerDto = new RegisterDto
        {
            Email = invalidEmail,
            Password = "TestPassword123!"
        };

        setup.Controller.ModelState.AddModelError("Email", "Invalid email format");

        // Act
        var result = await setup.Controller.RegisterAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
    }

    [Fact]
    public async Task RegisterAsync__Should_Return_CreatedAtAction_With_CorrectRouteValues()
    {
        // Arrange
        using var setup = AuthApiControllerTestSetup.Create();

        var registerDto = new RegisterDto
        {
            Email = "routetest@example.com",
            Password = "TestPassword123!"
        };

        // Act
        var result = await setup.Controller.RegisterAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        var createdAtActionResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;

        createdAtActionResult.ActionName.Should().Be("LoginAsync");
        createdAtActionResult.RouteValues.Should().NotBeNull();
        createdAtActionResult.RouteValues.Should().ContainKey("username");
        createdAtActionResult.RouteValues["username"].Should().Be("routetest@example.com");
        createdAtActionResult.Value.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync__Should_SetUserName_SameAsEmail()
    {
        // Arrange
        using var setup = AuthApiControllerTestSetup.Create();

        var registerDto = new RegisterDto
        {
            Email = "username.test@example.com",
            Password = "TestPassword123!"
        };

        // Act
        await setup.Controller.RegisterAsync(registerDto);

        // Assert
        var user = await setup.UserManager.FindByEmailAsync("username.test@example.com");
        user.Should().NotBeNull();
        user!.UserName.Should().Be("username.test@example.com");
        user.Email.Should().Be("username.test@example.com");
        user.UserName.Should().Be(user.Email); // Verify they are the same
    }



}

