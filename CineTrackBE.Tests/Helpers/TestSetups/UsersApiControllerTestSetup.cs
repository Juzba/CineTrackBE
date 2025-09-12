using CineTrackBE.ApiControllers;
using CineTrackBE.AppServices;
using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace CineTrackBE.Tests.Helpers.TestSetups;

public class UsersApiControllerTestSetup : IDisposable
{
    public ApplicationDbContext Context { get; }
    public UsersApiController Controller { get; }
    public ILogger<UsersApiController> Logger { get; }
    public IRepository<ApplicationUser> UserRepository { get; }
    public IRepository<IdentityUserRole<string>> UserRoleRepository { get; }
    public IRepository<IdentityRole> RoleRepository { get; }

    private UsersApiControllerTestSetup(
        ApplicationDbContext context,
        UsersApiController controller,
        ILogger<UsersApiController> logger,
        IRepository<IdentityUserRole<string>> userRoleRepository,
        IRepository<ApplicationUser> userRepository,
        IRepository<IdentityRole> roleRepository
        )
    {
        Context = context;
        Controller = controller;
        Logger = logger;
        UserRoleRepository = userRoleRepository;
        UserRepository = userRepository;
        RoleRepository = roleRepository;
    }

    public static UsersApiControllerTestSetup Create(
        ApplicationDbContext? context = null,
        IRepository<IdentityUserRole<string>>? userRoleRepository = null,
        IRepository<ApplicationUser>? userRepository = null,
        IRepository<IdentityRole>? roleRepository = null
    )
    {
        context ??= DatabaseTestHelper.CreateSqlLiteContext(false);

        userRoleRepository ??= new Repository<IdentityUserRole<string>>(context, new Mock<ILogger<Repository<IdentityUserRole<string>>>>().Object);
        userRepository ??= new Repository<ApplicationUser>(context, new Mock<ILogger<Repository<ApplicationUser>>>().Object);
        roleRepository ??= new Repository<IdentityRole>(context, new Mock<ILogger<Repository<IdentityRole>>>().Object);

        var logger = new Mock<ILogger<UsersApiController>>().Object;

        var controller = new UsersApiController(roleRepository, userRepository, userRoleRepository, logger);

        return new UsersApiControllerTestSetup(
            context,
            controller,
            logger,
            userRoleRepository,
            userRepository,
            roleRepository
            );
    }

    public void Dispose()
    {
        Context?.Dispose();
    }
}
