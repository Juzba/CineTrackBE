using CineTrackBE.AppServices;
using CineTrackBE.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

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
              .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
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