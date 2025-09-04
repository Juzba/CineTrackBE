using CineTrackBE.ApiControllers;
using CineTrackBE.AppServices;
using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using CineTrackBE.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

public class FilmApiControllerTestSetup : IDisposable
{
    public ApplicationDbContext Context { get; }
    public FilmApiController Controller { get; }
    public Mock<ILogger<FilmApiController>> LoggerMock { get; }


    public Repository<Film> FilmRepository { get; }
    public Repository<Rating> RatingRepository { get; }
    public Repository<Comment> CommentRepository { get; }
    public Repository<ApplicationUser> UserRepository { get; }
    public Repository<Genre> GenreRepository { get; }

    private FilmApiControllerTestSetup(
        ApplicationDbContext context,
        FilmApiController controller,
        Mock<ILogger<FilmApiController>> loggerMock,
        Repository<Film> filmRepository,
        Repository<Rating> ratingRepository,
        Repository<Comment> commentRepository,
        Repository<ApplicationUser> userRepository,
        Repository<Genre> genreRepository)
    {
        Context = context;
        Controller = controller;
        LoggerMock = loggerMock;
        FilmRepository = filmRepository;
        RatingRepository = ratingRepository;
        CommentRepository = commentRepository;
        UserRepository = userRepository;
        GenreRepository = genreRepository;
    }

    public static FilmApiControllerTestSetup Create()
    {
        var context = DatabaseTestHelper.CreateSqlLiteContext();
        var loggerMock = new Mock<ILogger<FilmApiController>>();

        var filmRepository = DatabaseTestHelper.CreateRepository<Film>(context);
        var ratingRepository = DatabaseTestHelper.CreateRepository<Rating>(context);
        var commentRepository = DatabaseTestHelper.CreateRepository<Comment>(context);
        var userRepository = DatabaseTestHelper.CreateRepository<ApplicationUser>(context);
        var genreRepository = DatabaseTestHelper.CreateRepository<Genre>(context);

        var controller = new FilmApiController(
            loggerMock.Object,
            filmRepository,
            ratingRepository,
            commentRepository,
            userRepository,
            genreRepository
        );

        return new FilmApiControllerTestSetup(
            context,
            controller,
            loggerMock,
            filmRepository,
            ratingRepository,
            commentRepository,
            userRepository,
            genreRepository
        );
    }

    public void Dispose()
    {
        Context?.Dispose();
    }
}