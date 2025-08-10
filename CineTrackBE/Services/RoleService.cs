using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.Services
{
    public interface IRoleService
    {

        Task<bool> AddUserRole(User user, string role);
        Task<bool> RemoveUserRole(User user, string role);
        Task<IEnumerable<IdentityRole>> GetRoles_FromUser(User user);
        Task<IEnumerable<IdentityUserRole<string>>> GetUserRole_List();
        Task<IEnumerable<IdentityRole<string>>> GetRole_List();
        Task<int> UsersInRole_Count(string role);
        Task SaveChangesAsync();




    }
    public class RoleService(ApplicationDbContext context) : IRoleService
    {
        private readonly ApplicationDbContext _context = context;


        public async Task<bool> AddUserRole(User user, string role)
        {
            if (user == null) throw new ArgumentNullException(nameof(user), "user is null!");
            if (string.IsNullOrEmpty(role)) throw new ArgumentNullException(nameof(role), "role is null or empty!");


            var myRole = await _context.Roles.FirstOrDefaultAsync(p => p.Name == role);
            if (myRole == null) return false;

            // this role in db?
            var myUserRole = await _context.UserRoles.Where(p => p.UserId == user.Id).FirstOrDefaultAsync(p => p.RoleId == myRole.Id);
            if (myUserRole != null) return true; // role is already in db

            var newRole = new IdentityUserRole<string>()
            {
                UserId = user.Id,
                RoleId = myRole.Id
            };

            await _context.UserRoles.AddAsync(newRole);
            return true;
        }

        // REMOVE USER-ROLE //
        public async Task<bool> RemoveUserRole(User user, string role)
        {
            if (user == null) throw new ArgumentNullException(nameof(user), "user is null!");
            if (string.IsNullOrEmpty(role)) throw new ArgumentNullException(nameof(role), "role is null or empty!");


            var myRole = await _context.Roles.FirstOrDefaultAsync(p => p.Name == role);
            if (myRole == null) return false;

            // this role in db?
            var myUserRole = await _context.UserRoles.Where(p => p.UserId == user.Id).FirstOrDefaultAsync(p => p.RoleId == myRole.Id);
            if (myUserRole == null) return true; // role is not in db

            _context.UserRoles.Remove(myUserRole);

            return true;
        }





        // GET ROLES FROM USER ID //
        public async Task<IEnumerable<IdentityRole>> GetRoles_FromUser(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user), "user is null!");


            List<IdentityRole> roles = [];

            var allUserRoles = await _context.UserRoles.ToListAsync();
            var allRoles = await _context.Roles.ToListAsync();

            var userRoles = allUserRoles.Where(p => p.UserId == user.Id);


            foreach (var item in userRoles)
            {
                var role = allRoles.FirstOrDefault(p => p.Id == item.RoleId);

                if (role != null) roles.Add(role);
            }

            return roles;
        }

        // ROLE ADMIN COUNT //
        public async Task<int> UsersInRole_Count(string role)
        {
            if (string.IsNullOrEmpty(role)) throw new ArgumentNullException(nameof(role), "role is null or empty!");


            var myRole = await _context.Roles.FirstOrDefaultAsync(p => p.Name == role);
            if (myRole == null) return 0;

            return _context.UserRoles.Count(p => p.RoleId == myRole.Id);
        }



        // GET USER-ROLE LIST //
        public async Task<IEnumerable<IdentityUserRole<string>>> GetUserRole_List() => await _context.UserRoles.ToListAsync();

        // GET ROLE LIST //
        public async Task<IEnumerable<IdentityRole<string>>> GetRole_List() => await _context.Roles.ToListAsync();


        // SAVE CHANGES //
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }


    }
}
