using CineTrackBE.AppServices;
using CineTrackBE.Models.Entities;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CineTrackBE.Tests.Unit.Services;

public class JwtServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        var configDict = new Dictionary<string, string>
        {
            ["Jwt:Key"] = "super-secret-key-that-is-at-least-32-characters-long",
            ["Jwt:Issuer"] = "TestIssuer",
            ["Jwt:Audience"] = "TestAudience",
            ["Jwt:ExpirationInMinutes"] = "60"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict!)
            .Build();

        _jwtService = new JwtService(_configuration);
    }

    [Fact]
    public void GenerateToken_ValidUser_ReturnsValidJwtToken()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "test-user-id",
            UserName = "testuser",
            Email = "test@example.com"
        };
        var roles = new List<string> { "Admin", "User" };

        // Act
        var token = _jwtService.GenerateToken(user, roles);

        // Assert
        token.Should().NotBeNullOrEmpty();

        var tokenHandler = new JwtSecurityTokenHandler();
        tokenHandler.CanReadToken(token).Should().BeTrue();

        var jwtToken = tokenHandler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "test-user-id");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "testuser");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "test@example.com");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");

        jwtToken.Issuer.Should().Be("TestIssuer");
        jwtToken.Audiences.Should().Contain("TestAudience");
        jwtToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void GenerateToken_UserWithNoRoles_ReturnsTokenWithoutRoleClaims()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-no-roles",
            UserName = "noroleuser",
            Email = "norole@example.com"
        };
        var roles = new List<string>();

        // Act
        var token = _jwtService.GenerateToken(user, roles);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).Should().BeEmpty();
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier);
    }

    [Fact]
    public void GenerateToken_ValidatesTokenSignature()
    {
        // Arrange
        var user = new ApplicationUser { Id = "1", UserName = "test", Email = "test@test.com" };

        // Act
        var token = _jwtService.GenerateToken(user, new List<string>());

        // Assert 
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = _configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var act = () => tokenHandler.ValidateToken(token, validationParameters, out _);
        act.Should().NotThrow();
    }
}
