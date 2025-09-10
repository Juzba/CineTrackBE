using CineTrackBE.ApiControllers;
using CineTrackBE.AppServices;
using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace CineTrackBE.Tests.Helpers.TestSetups;

public class AdminApiControllerTestSetup : IDisposable
{
    public ApplicationDbContext Context { get; }
    public AdminApiController Controller { get; }
    public ILogger<AdminApiController> Logger { get; }
    public IRepository<Film> FilmRepository { get; }
    public IRepository<IdentityUserRole<string>> UserRoleRepository { get; }
    public IRepository<Genre> GenreRepository { get; }
    public IRepository<FilmGenre> FilmGenreRepository { get; }

    private AdminApiControllerTestSetup(
        ApplicationDbContext context,
        AdminApiController controller,
        ILogger<AdminApiController> logger,
        IRepository<IdentityUserRole<string>> userRoleRepository,
        IRepository<Film> filmRepository,
        IRepository<Genre> genreRepository,
        IRepository<FilmGenre> filmGenreRepository
        )
    {
        Context = context;
        Controller = controller;
        Logger = logger;
        UserRoleRepository = userRoleRepository;
        FilmRepository = filmRepository;
        GenreRepository = genreRepository;
        FilmGenreRepository = filmGenreRepository;
    }

    public static AdminApiControllerTestSetup Create(
        ApplicationDbContext? context = null,
        IRepository<IdentityUserRole<string>>? userRoleRepository = null,
        IRepository<Genre>? genreRepository = null,
        IRepository<Film>? filmRepository = null,
        IRepository<FilmGenre>? filmGenreRepository = null
    )
    {
        context ??= DatabaseTestHelper.CreateSqlLiteContext(false);

        userRoleRepository ??= new Repository<IdentityUserRole<string>>(context, new Mock<ILogger<Repository<IdentityUserRole<string>>>>().Object);
        filmRepository ??= new Repository<Film>(context, new Mock<ILogger<Repository<Film>>>().Object);
        genreRepository ??= new Repository<Genre>(context, new Mock<ILogger<Repository<Genre>>>().Object);
        filmGenreRepository ??= new Repository<FilmGenre>(context, new Mock<ILogger<Repository<FilmGenre>>>().Object);

        var logger = new Mock<ILogger<AdminApiController>>().Object;

        var controller = new AdminApiController(
            userRoleRepository,
            logger,
            filmRepository,
            genreRepository,
            filmGenreRepository);

        return new AdminApiControllerTestSetup(
            context,
            controller,
            logger,
            userRoleRepository,
            filmRepository,
            genreRepository,
            filmGenreRepository
            );
    }

    public void Dispose()
    {
        Context?.Dispose();
    }
}
