using CineTrackBE.ApiControllers;
using CineTrackBE.Tests.Helpers.TestSetups;
using Microsoft.AspNetCore.Identity.Data;
using Xunit;

namespace CineTrackBE.Tests.Integration.ApiEndpoints;

public class AuthApiControllerTests
{
    //[Fact]
    //public async Task LoginAsync_Should_Return_BadRequest_When_ModelStateIsNotValid()
    //{
        //// Arrange
        //var invalidLoginModel = new LoginRequest
        //{
        //    Email = "invalid-email",
        //    Password = "short"
        //};

        //var httpContext = HttpContextTestSetup.Create().Build();
        ////httpContext.Request.Headers["Content-Type"] = "application/json";
        ////httpContext.Request.Body = GenerateStreamFromString(JsonConvert.SerializeObject(invalidLoginModel));

        //var controller = new AuthApiController(_authServiceMock.Object, _jwtServiceMock.Object)
        //{
        //    ControllerContext = new ControllerContext
        //    {
        //        HttpContext = httpContext
        //    }
        //};

        //// Act
        //var result = await controller.LoginAsync(invalidLoginModel);

        //// Assert
        //var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        //Assert.NotNull(badRequestResult.Value);
    //}


    // LoginAsync
    // modelstate valid
    // user not exist in db
    // invalid user or password
    // success login with token and userdto



}
