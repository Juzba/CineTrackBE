using CineTrackBE.ApiControllers;
using CineTrackBE.AppServices;
using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace CineTrackBE.Tests.Helpers.TestSetups;

public class RepositoryTestSetup: IDisposable
{

    public ApplicationDbContext Context { get; }
    public Mock<ILogger<FilmApiController>> LoggerMock { get; }
    public Repository<Film> FilmRepository { get; }
    public Repository<IdentityRole> RoleRepository { get; }


    private RepositoryTestSetup(ApplicationDbContext context, Mock<ILogger<FilmApiController>> loggerMock, Repository<Film> filmRepository, Repository<IdentityRole> roleRepository)
    {
        Context = context;
        LoggerMock = loggerMock;
        FilmRepository = filmRepository;
        RoleRepository = roleRepository;
    }

    public static RepositoryTestSetup Create()
    {
        var context = DatabaseTestHelper.CreateSqlLiteContext();
        var loggerMock = new Mock<ILogger<FilmApiController>>();

        var filmRepository = DatabaseTestHelper.CreateRepository<Film>(context);
        var roleRepository = DatabaseTestHelper.CreateRepository<IdentityRole>(context);


        return new RepositoryTestSetup(context, loggerMock, filmRepository, roleRepository);
    }

    public void Dispose()
    {
        Context?.Dispose();
    }



}
