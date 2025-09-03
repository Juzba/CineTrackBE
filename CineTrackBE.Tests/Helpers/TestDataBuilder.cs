using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace CineTrackBE.Tests.Helpers
{
    public class TestDataBuilder
    {
        public class RoleBuilder
        {
            private IdentityRole _role = new IdentityRole();
            private List<IdentityRole> _roleList = [];

            public static RoleBuilder Create() => new RoleBuilder();

            public RoleBuilder WithName(string name)
            {
                _role.Name = name;
                _role.NormalizedName = name.ToUpper();
                return this;
            }

            public RoleBuilder WithRandomData()
            {
                var randomId = Guid.NewGuid().ToString()[..8];
                _role.Name = $"TestRole_{randomId}";
                _role.NormalizedName = _role.Name.ToUpper();
                _role.ConcurrencyStamp = Guid.NewGuid().ToString();
                return this;
            }

            public RoleBuilder ListWithRandomData(int count)
            {
                for (int i = 0; i < count; i++)
                {
                    _role = new();
                    WithRandomData();
                    _roleList.Add(_role);
                }

                return this;
            }

            public IdentityRole Build() => _role;

            public List<IdentityRole> BuildList() => _roleList;

            public async Task<IdentityRole> BuildAndSaveAsync(ApplicationDbContext context)
            {
                context.Roles.Add(_role);
                await context.SaveChangesAsync();
                return _role;
            }

            public async Task<List<IdentityRole>> BuildListAndSaveAsync(ApplicationDbContext context)
            {
                context.Roles.AddRange(_roleList);
                await context.SaveChangesAsync();
                return _roleList;
            }
        }



        public class FilmBuilder
        {
            private Film _film = new Film();
            private List<Film> _filmList = [];

            public static FilmBuilder Create() => new FilmBuilder();

            public FilmBuilder WithId(int id)
            {
                _film.Id = id;
                return this;
            }

            public FilmBuilder WithName(string name)
            {
                _film.Name = name;
                return this;
            }

            public FilmBuilder WithDirector(string name)
            {
                _film.Director = name;
                return this;
            }

            public FilmBuilder WithYear(int year)
            {
                var dateTime = new DateTime(year, 1, 1);
                _film.ReleaseDate = dateTime;
                return this;
            }

            public FilmBuilder WithRandomData()
            {
                _film.Name = $"Test Film {Guid.NewGuid().ToString()[..8]}";
                _film.Director = $"Test Director {Guid.NewGuid().ToString()[..8]}";

                var randomYear = Random.Shared.Next(1990, 2024);
                var randomMonth = Random.Shared.Next(1, 13);
                var randomDay = Random.Shared.Next(1, 28);

                _film.ReleaseDate = new DateTime(randomYear, randomMonth, randomDay);
                return this;
            }
            public FilmBuilder ListWithRandomData(int count)
            {
                for (int i = 0; i < count; i++)
                {
                    _film = new();
                    WithRandomData();
                    _filmList.Add(_film);
                }

                return this;
            }

            public Film Build() => _film;

            public List<Film> BuildList() => _filmList;


            public async Task<Film> BuildAndSaveAsync(ApplicationDbContext context)
            {
                context.Films.Add(_film);
                await context.SaveChangesAsync();
                return _film;
            }

            public async Task<List<Film>> BuildListAndSaveAsync(ApplicationDbContext context)
            {
                context.Films.AddRange(_filmList);
                await context.SaveChangesAsync();
                return _filmList;
            }
        }

    }
}
