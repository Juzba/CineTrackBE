using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.AppServices
{
    public interface IDataService
    {
        Task AddUserRoleAsync(ApplicationUser user, string role, CancellationToken cancellationToken = default);
        Task RemoveUserRoleAsync(ApplicationUser user, string role, CancellationToken cancellationToken = default);
        Task<IEnumerable<IdentityRole>> GetRolesFromUserAsync(ApplicationUser user, CancellationToken cancellationToken = default);
        Task<int> CountUserInRoleAsync(string role, CancellationToken cancellationToken = default);
        Task AddGenresToFilmAsync(Film film, List<int> genreIds, CancellationToken cancellationToken = default);
    }
    public class DataService(ApplicationDbContext context, ILogger<DataService> logger) : IDataService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<DataService> _logger = logger;



        // ADD USER ROLE //
        public async Task AddUserRoleAsync(ApplicationUser user, string role, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentException.ThrowIfNullOrWhiteSpace(role);

            var roleEntity = await _context.Roles.FirstOrDefaultAsync(r => r.Name == role, cancellationToken)
            ?? throw new ArgumentException("Role does not exist.", nameof(role));

            // any user-role in db? //
            var exist = await _context.UserRoles.AnyAsync(p => p.UserId == user.Id && p.RoleId == roleEntity.Id, cancellationToken);
            if (exist) throw new ArgumentException("The user already has this role assigned.", nameof(role));


            var userRole = new IdentityUserRole<string> { UserId = user.Id, RoleId = roleEntity.Id };
            await _context.UserRoles.AddAsync(userRole, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Role {Role} added to user {UserName}", role, user.UserName);
        }


        // REMOVE USER ROLE //
        public async Task RemoveUserRoleAsync(ApplicationUser user, string role, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentException.ThrowIfNullOrWhiteSpace(role);

            var roleEntity = await _context.Roles.FirstOrDefaultAsync(r => r.Name == role, cancellationToken)
            ?? throw new ArgumentException("Role does not exist.", nameof(role));


            var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == roleEntity.Id, cancellationToken)
            ?? throw new ArgumentException("The user does not have this role assigned.", nameof(role));


            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Role {Role} removed from user {UserName}", role, user.UserName);
        }


        // GET ROLES FROM USER //
        public async Task<IEnumerable<IdentityRole>> GetRolesFromUserAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(user);

            var userRoles = await _context.UserRoles.Where(ur => ur.UserId == user.Id).Select(ur => ur.RoleId).ToListAsync(cancellationToken);
            return await _context.Roles.Where(r => userRoles.Contains(r.Id)).ToListAsync(cancellationToken);
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
            ArgumentNullException.ThrowIfNull(genreIds);

            if (genreIds.Count == 0)
            {
                _logger.LogWarning("No genre IDs provided to add to film with ID {FilmId}", film.Id);
                throw new ArgumentException("The genreIds list is empty.", nameof(genreIds));
            }

            // film-genres existing in db //
            var existsFilmGenres = await _context.FilmGenres.Where(p => p.FilmId == film.Id && genreIds.Contains(p.GenreId)).ToListAsync(cancellationToken);

            // film-genres not existing in db //
            var genreList = genreIds.Except(existsFilmGenres.Select(p => p.GenreId)).Select(p => new FilmGenre() { GenreId = p, FilmId = film.Id });

            if (genreList == null || !genreList.Any())
            {
                _logger.LogWarning("No new genres to add for film with ID {FilmId}", film.Id);
                return;
            }

            await _context.AddRangeAsync(genreList, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

    }
}
