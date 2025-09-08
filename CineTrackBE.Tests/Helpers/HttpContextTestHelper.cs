using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging;
using System.Security.Claims;

namespace CineTrackBE.Tests.Helpers
{
    public class HttpContextTestHelper
    {
        public (DefaultHttpContext HttpContext, ClaimsPrincipal ClaimsPrincipal) CreateHttpContext(
            string? userId = null,
            string? userName = null,
            string? role = null,
            IEnumerable<Claim>? additionalClaims = null
            )
        {
            userId ??= "test-user-id";
            userName ??= "testuser";
            role ??= "Admin";

            var defaultClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, role)
            };

            if (additionalClaims != null)
            {
                defaultClaims.AddRange(additionalClaims);
            }

            var identity = new ClaimsIdentity(defaultClaims, JwtBearerDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };

            return (httpContext, claimsPrincipal);
        }


        public static void SetupControllerContext<T>(T controller, DefaultHttpContext httpContext) where T : ControllerBase
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }
    }
}
