using CineTrackBE.ApiControllers;
using CineTrackBE.AppServices;
using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using CineTrackBE.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

public class FilmApiControllerTestSetup : IDisposable
{
    public ApplicationDbContext Context { get; }
    public FilmApiController Controller { get; }
    public DefaultHttpContext _httpContext { get; }


    public Mock<ILogger<FilmApiController>> LoggerMock { get; }
    public IRepository<Film> FilmRepository { get; }
    public IRepository<Rating> RatingRepository { get; }
    public IRepository<Comment> CommentRepository { get; }
    public IRepository<ApplicationUser> UserRepository { get; }
    public IRepository<Genre> GenreRepository { get; }

    private FilmApiControllerTestSetup(
        ApplicationDbContext context,
        FilmApiController controller,
        Mock<ILogger<FilmApiController>> loggerMock,
        IRepository<Film> filmRepository,
        IRepository<Rating> ratingRepository,
        IRepository<Comment> commentRepository,
        IRepository<ApplicationUser> userRepository,
        IRepository<Genre> genreRepository,
        DefaultHttpContext httpContext)
    {
        Context = context;
        Controller = controller;
        LoggerMock = loggerMock;
        FilmRepository = filmRepository;
        RatingRepository = ratingRepository;
        CommentRepository = commentRepository;
        UserRepository = userRepository;
        GenreRepository = genreRepository;
        _httpContext = httpContext;
    }

    public static FilmApiControllerTestSetup Create
        (
        ApplicationDbContext? context = null,
        IRepository<Genre>? genreRepository = null,
        IRepository<Film>? filmRepository = null,
        string? userId = null,
        string? userName = null)
    {
        context ??= DatabaseTestHelper.CreateSqlLiteContext();

        var loggerMock = new Mock<ILogger<FilmApiController>>();

        filmRepository ??= DatabaseTestHelper.CreateRepository<Film>(context);
        var ratingRepository = DatabaseTestHelper.CreateRepository<Rating>(context);
        var commentRepository = DatabaseTestHelper.CreateRepository<Comment>(context);
        var userRepository = DatabaseTestHelper.CreateRepository<ApplicationUser>(context);
        genreRepository ??= DatabaseTestHelper.CreateRepository<Genre>(context);


        var controller = new FilmApiController(
            loggerMock.Object,
            filmRepository,
            ratingRepository,
            commentRepository,
            userRepository,
            genreRepository
        );
        var (httpContext, _) = DatabaseTestHelper.CreateHttpContext(userId, userName);
        DatabaseTestHelper.SetupControllerContext(controller, httpContext);

        return new FilmApiControllerTestSetup(
            context,
            controller,
            loggerMock,
            filmRepository,
            ratingRepository,
            commentRepository,
            userRepository,
            genreRepository,
            httpContext
        );
    }


    public void Dispose()
    {
        Context?.Dispose();
    }

}