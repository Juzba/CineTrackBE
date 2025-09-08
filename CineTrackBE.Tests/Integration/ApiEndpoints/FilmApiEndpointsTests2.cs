using CineTrackBE.Models.DTO;
using CineTrackBE.Models.Entities;
using CineTrackBE.Tests.Helpers.Common;
using CineTrackBE.Tests.Helpers.TestSetups;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Xunit;

namespace CineTrackBE.Tests.Integration.ApiEndpoints
{
    public class FilmApiEndpointsTests2
    {
        [Fact]
        public async Task GetFilm__Should_Return_OkResult_WithFilm()
        {
            // Arrange
            using var setup = FilmApiControllerTestSetup.Create();

            var testUser = HttpContextTestSetup.Create().BuildAndSave(setup.Controller);

            // zredukovat

            //var testUserId = "test-user-id";
            //var claims = new[]
            //{
            //    new Claim(ClaimTypes.NameIdentifier, testUserId),
            //    new Claim(ClaimTypes.Name, "TestUser")
            //};
            //var identity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
            //var claimsPrincipal = new ClaimsPrincipal(identity);

            //setup._httpContext.User = claimsPrincipal;

            ////////////////////////////////////

            // celokove zkusit prekopat ten kontext

            // Připravíme testovacího uživatele v databázi


            // nastavit do fakera 

            var user = new ApplicationUser
            {
                Id = testUserId,
                UserName = "TestUser",
                PasswordHash = "Hash",
                FavoriteMovies = [] // prázdný seznam oblíbených filmů
            };
            await setup.UserRepository.AddAsync(user);


            //////////////////////////////


            var films = await Fakers.Film.GenerateAndSaveAsync(3, setup.Context);
            var targetedFilm = films[1];

            // Nastavíme ClaimsPrincipal pro request



            // Act
            var result = await setup.Controller.GetFilm(targetedFilm.Id);

            // Assert
            result.Should().NotBeNull();
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeAssignableTo<FilmDto>();
        }




















    }
}
