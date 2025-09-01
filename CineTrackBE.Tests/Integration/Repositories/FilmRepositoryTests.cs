using CineTrackBE.AppServices;
using CineTrackBE.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace CineTrackBE.Tests.Integration.Repositories
{
    public class FilmRepositoryTests
    {
        //private DataService _dataService;
        private ApplicationDbContext _context;

        [SetUp]
        public void Setup()
        {

            var option = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            _context = new ApplicationDbContext(option);
           
            
            //_dataService = new DataService(_context);

        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

    }
}
