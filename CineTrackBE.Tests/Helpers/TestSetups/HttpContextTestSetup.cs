using CineTrackBE.AppServices;
using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CineTrackBE.Tests.Helpers.TestSetups;
public class HttpContextTestSetup
{
    private readonly HttpContextTestHelper _httpContextTestHelper;
    private string? _userId;
    private string? _userName;
    private string? _role;
    private readonly List<Claim> _additionalClaims = [];

    private HttpContextTestSetup()
    {
        _httpContextTestHelper = new HttpContextTestHelper();
    }

    public static HttpContextTestSetup Create()
    {
        return new HttpContextTestSetup();
    }

    public HttpContextTestSetup WithUser(string? userId = null, string? userName = null, string? role = null)
    {
        _userId = userId;
        _userName = userName;
        _role = role;
        return this;
    }

    public HttpContextTestSetup WithClaim(string type, string value)
    {
        _additionalClaims.Add(new Claim(type, value));
        return this;
    }


    public (ApplicationUser User, DefaultHttpContext HttpContext, ClaimsPrincipal Claims) Build<T>(T controller) where T : ControllerBase
    {
        var (httpContext, claimsPrincipal) = _httpContextTestHelper.CreateHttpContext(_userId, _userName, _role, _additionalClaims);
        HttpContextTestHelper.SetupControllerContext(controller, httpContext);

        var user = new ApplicationUser
        {
            Id = _userId ?? claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)!,
            UserName = _userName ?? claimsPrincipal.FindFirstValue(ClaimTypes.Name),
            NormalizedUserName = _userName?.ToUpper() ?? claimsPrincipal.FindFirstValue(ClaimTypes.Name)?.ToUpper(),
            Email = _userName ?? claimsPrincipal.FindFirstValue(ClaimTypes.Name),
            NormalizedEmail = _userName?.ToUpper() ?? claimsPrincipal.FindFirstValue(ClaimTypes.Name)?.ToUpper(),
            PasswordHash = "123456-hash",
            EmailConfirmed = true,
        };
        return (User: user, HttpContext: httpContext, Claims: claimsPrincipal);
    }


    public async Task<(ApplicationUser User, DefaultHttpContext HttpContext, ClaimsPrincipal Claims)> BuildAndSaveAsync<T>(T controller, ApplicationDbContext context, CancellationToken cancellationToken = default) where T : ControllerBase
    {
        var (user, httpContext, claimsPrincipal) = Build(controller);

        var exist = await context.Users.FindAsync([user.Id], cancellationToken);
        if (exist == null)
        {
            await context.AddAsync(user, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        return (User: user, HttpContext: httpContext, Claims: claimsPrincipal);
    }
}