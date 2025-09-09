using CineTrackBE.ApiControllers;
using CineTrackBE.AppServices;
using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace CineTrackBE.Tests.Helpers.TestSetups;

public class AuthApiControllerTestSetup : IDisposable
{
    public AuthApiController Controller { get; private set; }
    private readonly ApplicationDbContext _context;

    private AuthApiControllerTestSetup(AuthApiController controller, ApplicationDbContext context)
    {
        Controller = controller;
        _context = context;
    }

    public static AuthApiControllerTestSetup Create()
    {
        // Logger mock
        var loggerMock = new Mock<ILogger<AuthApiController>>();

        // Setup test DB context
        var context = DatabaseTestHelper.CreateSqlLiteContext(false);

        // UserStore mock
        var userStore = new Mock<IUserStore<ApplicationUser>>();

        // UserManager mock
        var userManager = new Mock<UserManager<ApplicationUser>>(
            userStore.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        // SignInManager mock
        var signInManager = new Mock<SignInManager<ApplicationUser>>(
            userManager.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
            null!, null!, null!, null!);

        // JWT configuration mock
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(x => x["JWT:ValidAudience"]).Returns("http://localhost:4200");
        configurationMock.Setup(x => x["JWT:ValidIssuer"]).Returns("http://localhost:5000");
        configurationMock.Setup(x => x["JWT:Secret"]).Returns("JWTAuthenticationSecretKey");

        // JwtService instance
        var jwtService = new JwtService(configurationMock.Object);

        // Create controller
        var controller = new AuthApiController(
            loggerMock.Object,
            userManager.Object,
            signInManager.Object,
            jwtService);

        return new AuthApiControllerTestSetup(controller, context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

// JwtService by měl implementovat IJwtService
public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        if (string.IsNullOrEmpty(_configuration["JWT:Secret"]) ||
            string.IsNullOrEmpty(_configuration["JWT:ValidIssuer"]) ||
            string.IsNullOrEmpty(_configuration["JWT:ValidAudience"]))
        {
            throw new ArgumentException("JWT configuration is missing required values");
        }
    }

    // Implementace požadované metody z rozhraní IJwtService
    public string GenerateToken(ApplicationUser user, IList<string> roles)
    {
        // Pro testovací účely můžeme vrátit dummy token
        return "test_token";
    }
}
