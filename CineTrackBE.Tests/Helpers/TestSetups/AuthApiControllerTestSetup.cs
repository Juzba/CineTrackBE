using CineTrackBE.ApiControllers;
using CineTrackBE.AppServices;
using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using CineTrackBE.Tests.Helpers.TestSetups.Universal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace CineTrackBE.Tests.Helpers.TestSetups
{

    public class AuthApiControllerTestSetup : IDisposable
    {
        public AuthApiController Controller { get; }
        public ApplicationDbContext Context { get; }
        public UserManager<ApplicationUser> UserManager { get; }
        public SignInManager<ApplicationUser> SignInManager { get; }
        public IJwtService JwtService { get; }

        private readonly InMemoryDbTestSetup _dbSetup;

        private AuthApiControllerTestSetup(
            AuthApiController controller,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtService jwtService,
            InMemoryDbTestSetup dbSetup)
        {
            Controller = controller;
            Context = context;
            UserManager = userManager;
            SignInManager = signInManager;
            JwtService = jwtService;
            _dbSetup = dbSetup;
        }

        public static AuthApiControllerTestSetup Create()
        {
            var dbSetup = InMemoryDbTestSetup.Create();
            var context = dbSetup.Context;

            // Setup services
            var services = new ServiceCollection();

            // Add DbContext
            services.AddSingleton(context);

            // Add logging
            services.AddLogging();
            services.AddSingleton(new Mock<ILogger<AuthApiController>>().Object);

            // Add required authentication services
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Add authentication scheme provider and related services
            services.AddAuthentication()
                .AddCookie(); // This adds IAuthenticationSchemeProvider and other required services

            // Add Identity with all required services
            services.AddIdentityCore<ApplicationUser>(options =>
            {
                // Relaxed password requirements for testing
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 1;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager<SignInManager<ApplicationUser>>()
            .AddDefaultTokenProviders();

            // Add JWT service
            var jwtService = CreateJwtService();
            services.AddSingleton(jwtService);

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();

            // Create HttpContext for SignInManager
            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider
            };

            var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            httpContextAccessor.HttpContext = httpContext;

            // Ensure database is created
            context.Database.EnsureCreated();

            // Get services
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var signInManager = serviceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
            var logger = serviceProvider.GetRequiredService<ILogger<AuthApiController>>();

            var controller = new AuthApiController(logger, userManager, signInManager, jwtService);

            return new AuthApiControllerTestSetup(controller, context, userManager, signInManager, jwtService, dbSetup);
        }

        private static IJwtService CreateJwtService()
        {
            var configDict = new Dictionary<string, string>
            {
                ["Jwt:Key"] = "super-secret-key-that-is-at-least-32-characters-long-for-testing",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpirationInMinutes"] = "60"
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict!)
                .Build();

            return new JwtService(configuration);
        }

        public async Task<ApplicationUser> CreateUserAsync(string email, string password, params string[] roles)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email
            };

            var result = await UserManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            foreach (var role in roles)
            {
                var roleExists = await Context.Roles.AnyAsync(r => r.Name == role);
                if (!roleExists)
                {
                    await Context.Roles.AddAsync(new IdentityRole(role));
                    await Context.SaveChangesAsync();
                }

                var roleResult = await Context.Roles.FirstOrDefaultAsync(p => p.Name == role);
                if (roleResult == null)
                {
                    throw new InvalidOperationException("Failed to add user to role");
                }
                Context.UserRoles.Add(new IdentityUserRole<string>() { RoleId = roleResult.Id, UserId = user.Id });
                await Context.SaveChangesAsync();
            }

            return user;
        }

        public void Dispose()
        {
            _dbSetup?.Dispose();
        }
    }

}
