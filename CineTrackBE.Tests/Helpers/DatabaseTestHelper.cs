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
    // SQL LITE //
    public static ApplicationDbContext CreateSqlLiteContext(bool enableSeeding = false)
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


    // EF IN-MEMORY-DB //
    public static ApplicationDbContext CreateInMemoryContext(bool enableSeeding = false)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
              .UseInMemoryDatabase(databaseName: "TestDb")
              .Options;

        var context = new ApplicationDbContext(options, false);

        return context;
    }


    // CREATE REPOSITORY //
    public static Repository<T> CreateRepository<T>(ApplicationDbContext context) where T : class
    {
        return new Repository<T>(context, Mock.Of<ILogger<Repository<T>>>());
    }
}


