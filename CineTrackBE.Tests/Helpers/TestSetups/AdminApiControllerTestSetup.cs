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
    public Repository<Film> FilmRepository { get; }
    public Repository<IdentityUserRole<string>> UserRoleRepository { get; }
    public Repository<Genre> GenreRepository { get; }
    public Repository<FilmGenre> FilmGenreRepository { get; }

    private AdminApiControllerTestSetup(
        ApplicationDbContext context,
        AdminApiController controller,
        ILogger<AdminApiController> logger,
        Repository<IdentityUserRole<string>> userRoleRepository,
        Repository<Film> filmRepository,
        Repository<Genre> genreRepository,
        Repository<FilmGenre> filmGenreRepository
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
        Repository<IdentityUserRole<string>>? userRoleRepository = null,
        Repository<Genre>? genreRepository = null,
        Repository<Film>? filmRepository = null,
        Repository<FilmGenre>? filmGenreRepository = null
    )
    {
        context ??= DatabaseTestHelper.CreateSqlLiteContext(false);

        userRoleRepository ??= new Repository<IdentityUserRole<string>>(context, new Mock<ILogger<Repository<IdentityUserRole<string>>>>().Object);
        var logger = new Mock<ILogger<AdminApiController>>().Object;
        filmRepository ??= new Repository<Film>(context, new Mock<ILogger<Repository<Film>>>().Object);
        genreRepository ??= new Repository<Genre>(context, new Mock<ILogger<Repository<Genre>>>().Object);
        filmGenreRepository ??= new Repository<FilmGenre>(context, new Mock<ILogger<Repository<FilmGenre>>>().Object);

        var controller = new AdminApiController(userRoleRepository, logger, filmRepository, genreRepository, filmGenreRepository);


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
