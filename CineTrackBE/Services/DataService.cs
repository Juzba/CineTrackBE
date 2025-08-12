using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.Services
{
    public interface IDataService
    {
        Task<bool> AnyUserExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default);
        Task<bool> AddUserRoleAsync(User user, string role, CancellationToken cancellationToken = default);
        Task<bool> RemoveUserRoleAsync(User user, string role, CancellationToken cancellationToken = default);
        Task<IQueryable<IdentityRole>> GetRolesFromUserAsync(User user, CancellationToken cancellationToken = default);
        Task<int> CountUserInRoleAsync(string role, CancellationToken cancellationToken = default);
        Task AddGenresToFilmAsync(Film film, List<int> genreIds, CancellationToken cancellationToken = default);
    }
    public class DataService(ApplicationDbContext context, ILogger<DataService> logger) : IDataService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<DataService> _logger = logger;






        // ANY USER EXIST? //
        public async Task<bool> AnyUserExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userName, nameof(userName));

            return await _context.Users.AnyAsync(p => p.UserName == userName.ToUpper(), cancellationToken);
        }


        // ADD USER ROLE //
        public async Task<bool> AddUserRoleAsync(User user, string role, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentException.ThrowIfNullOrWhiteSpace(role);

            var roleEntity = await _context.Roles.FirstOrDefaultAsync(r => r.Name == role, cancellationToken);
            if (roleEntity == null) return false;

            // any user-role in db? //
            var anyExist = await _context.UserRoles.AnyAsync(p => p.UserId == user.Id && p.RoleId == roleEntity.Id);
            if (anyExist) return true;


            var userRole = new IdentityUserRole<string> { UserId = user.Id, RoleId = roleEntity.Id };
            await _context.UserRoles.AddAsync(userRole, cancellationToken);
            _logger.LogInformation("Role {Role} added to user {UserName}", role, user.UserName);


            return true;
        }


        // REMOVE USER ROLE //
        public async Task<bool> RemoveUserRoleAsync(User user, string role, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentException.ThrowIfNullOrWhiteSpace(role);

            var roleEntity = await _context.Roles.FirstOrDefaultAsync(r => r.Name == role, cancellationToken);
            if (roleEntity == null) return false;

            var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == roleEntity.Id, cancellationToken);
            if (userRole == null) return false;

            _context.UserRoles.Remove(userRole);
            _logger.LogInformation("Role {Role} removed from user {UserName}", role, user.UserName);

            return true;
        }


        // GET ROLES FROM USER //
        public async Task<IQueryable<IdentityRole>> GetRolesFromUserAsync(User user, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(user);

            var userRoles = await _context.UserRoles.Where(ur => ur.UserId == user.Id).Select(ur => ur.RoleId).ToListAsync(cancellationToken);
            return _context.Roles.Where(r => userRoles.Contains(r.Id)).AsQueryable();
        }


        // COUNT USER IN ROLE //
        public async Task<int> CountUserInRoleAsync(string role, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(role);

            var roleEntity = await _context.Roles.FirstOrDefaultAsync(r => r.Name == role, cancellationToken);
            if (roleEntity == null) return 0;

            return await _context.UserRoles.CountAsync(ur => ur.RoleId == roleEntity.Id, cancellationToken);
        }


        // ADD LIST OF GENRES TO FILM //
        public async Task AddGenresToFilmAsync(Film film, List<int> genreIds, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(film);

            if (genreIds == null || genreIds.Count == 0) return;

            // film-genres existing in db //
            var existsFilmGenres = await _context.FilmGenres.Where(p => p.FilmId == film.Id && genreIds.Contains(p.GenreId)).ToListAsync(cancellationToken);

            // film-genres not existing in db //
            var genreList = genreIds.Except(existsFilmGenres.Select(p => p.GenreId)).Select(p => new FilmGenre() { GenreId = p, FilmId = film.Id });

            if (genreList == null) return;
            await _context.AddRangeAsync(genreList, cancellationToken);

        }
    }
}
