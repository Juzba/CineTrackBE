using CineTrackBE.AppServices;
using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace CineTrackBE.Tests.Helpers;

public static class DatabaseTestHelper
{

    public static ApplicationDbContext CreateInMemoryContext(bool enableSeeding = false)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .EnableSensitiveDataLogging()
            .LogTo(Console.WriteLine, LogLevel.Information)
            .Options;

        var context = new ApplicationDbContext(options, enableSeeding);
        context.Database.EnsureCreated();

        return context;
    }

    public static Repository<T> CreateRepository<T>(ApplicationDbContext context) where T : class
    {
        return new Repository<T>(context, Mock.Of<ILogger<Repository<T>>>());
    }

    public static TestDatabaseSetup CreateTestSetup()
    {
        var context = CreateInMemoryContext();
        return new TestDatabaseSetup
        {
            Context = context,
            FilmRepository = CreateRepository<Film>(context),
            RoleRepository = CreateRepository<IdentityRole>(context)
        };
    }
}

public class TestDatabaseSetup : IDisposable
{
#nullable disable

    public ApplicationDbContext Context { get; init; }
    public Repository<Film> FilmRepository { get; init; }
    public Repository<IdentityRole> RoleRepository { get; init; }

    public void Dispose()
    {
        Context?.Dispose();
    }
}


