using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.Services
{
    public interface IRoleService
    {

        Task<bool> AddUserRoleAsync(User user, string role, CancellationToken cancellationToken = default);
        Task<bool> RemoveUserRoleAsync(User user, string role, CancellationToken cancellationToken = default);
        Task<IQueryable<IdentityRole>> GetRolesFromUserAsync(User user, CancellationToken cancellationToken = default);
        Task<int> CountUserInRoleAsync(string role, CancellationToken cancellationToken = default);
        IQueryable<IdentityUserRole<string>> GetUserRoleList();
        IQueryable<IdentityRole> GetRoleList();
        Task SaveChangesAsync(CancellationToken cancellationToken = default);

    }
    public class RoleService : IRoleService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RoleService> _logger;


        public RoleService(ApplicationDbContext context, ILogger<RoleService> logger)
        {
            _context = context;
            _logger = logger;
        }


        // ADD USER ROLE //
        public async Task<bool> AddUserRoleAsync(User user, string role, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentException.ThrowIfNullOrWhiteSpace(role);

            var roleEntity = await _context.Roles.FirstOrDefaultAsync(r => r.Name == role, cancellationToken);
            if (roleEntity == null) return false;

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

        // GET USER-ROLE LIST //
        public IQueryable<IdentityUserRole<string>> GetUserRoleList() => _context.UserRoles.AsQueryable();


        // GET ROLE LIST//
        public IQueryable<IdentityRole> GetRoleList() => _context.Roles.AsQueryable();


        // SAVE CHANGES //
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default) => await _context.SaveChangesAsync(cancellationToken);
    }
}