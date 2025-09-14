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
    public IRepository<ApplicationUser> UserRepository { get; }
    public IRepository<IdentityUserRole<string>> UserRoleRepository { get; }
    public IRepository<Genre> GenreRepository { get; }
    public IRepository<FilmGenre> FilmGenreRepository { get; }
    public IRepository<Comment> CommentRepository { get; }
    public IRepository<Rating> RatingRepository { get; }

    private AdminApiControllerTestSetup(
        ApplicationDbContext context,
        AdminApiController controller,
        ILogger<AdminApiController> logger,
        IRepository<IdentityUserRole<string>> userRoleRepository,
        IRepository<Film> filmRepository,
        IRepository<Genre> genreRepository,
        IRepository<FilmGenre> filmGenreRepository,
        IRepository<ApplicationUser> userRepository,
        IRepository<Comment> commentRepository,
        IRepository<Rating> ratingRepository 
        )
    {
        Context = context;
        Controller = controller;
        Logger = logger;
        UserRoleRepository = userRoleRepository;
        FilmRepository = filmRepository;
        GenreRepository = genreRepository;
        FilmGenreRepository = filmGenreRepository;
        UserRepository = userRepository;
        CommentRepository = commentRepository;
        RatingRepository = ratingRepository;
    }

    public static AdminApiControllerTestSetup Create(
        ApplicationDbContext? context = null,
        IRepository<IdentityUserRole<string>>? userRoleRepository = null,
        IRepository<Genre>? genreRepository = null,
        IRepository<Film>? filmRepository = null,
        IRepository<FilmGenre>? filmGenreRepository = null,
        IRepository<ApplicationUser>? userRepository = null,
        IRepository<Comment>? commentRepository = null,
        IRepository<Rating>? ratingRepository = null
    )
    {
        context ??= DatabaseTestHelper.CreateSqlLiteContext(false);

        userRoleRepository ??= new Repository<IdentityUserRole<string>>(context, new Mock<ILogger<Repository<IdentityUserRole<string>>>>().Object);
        filmRepository ??= new Repository<Film>(context, new Mock<ILogger<Repository<Film>>>().Object);
        genreRepository ??= new Repository<Genre>(context, new Mock<ILogger<Repository<Genre>>>().Object);
        filmGenreRepository ??= new Repository<FilmGenre>(context, new Mock<ILogger<Repository<FilmGenre>>>().Object);
        userRepository ??= new Repository<ApplicationUser>(context, new Mock<ILogger<Repository<ApplicationUser>>>().Object);
        commentRepository ??= new Repository<Comment>(context, new Mock<ILogger<Repository<Comment>>>().Object);
        ratingRepository ??= new Repository<Rating>(context, new Mock<ILogger<Repository<Rating>>>().Object);

        var logger = new Mock<ILogger<AdminApiController>>().Object;

        var controller = new AdminApiController(
            userRoleRepository,
            logger,
            filmRepository,
            genreRepository,
            filmGenreRepository,
            userRepository,
            ratingRepository,
            commentRepository
        );

        return new AdminApiControllerTestSetup(
            context,
            controller,
            logger,
            userRoleRepository,
            filmRepository,
            genreRepository,
            filmGenreRepository,
            userRepository,
            commentRepository,
            ratingRepository
            );
    }

    public void Dispose()
    {
        Context?.Dispose();
    }
}
