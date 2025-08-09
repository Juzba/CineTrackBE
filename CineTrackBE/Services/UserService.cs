using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.Services
{
    public interface IUserService
    {
        Task AddUser(User user);
        Task<User?> GetUser(string id);
        void UpdateUser(User user);
        void RemoveUser(User user);
        Task<IEnumerable<User>> GetUsersList();
        bool AnyUserExists_Id(string id);
        bool AnyUserExists_UserName(string userName);
        Task SaveChangesAsync();


        Task<bool> AddUserRole(User user, string role);
        Task<IEnumerable<IdentityRole>> GetRoles_FromUser(User user);
        Task<IEnumerable<IdentityUserRole<string>>> GetUserRole_List();
        Task<IEnumerable<IdentityRole<string>>> GetRole_List();


    }



    public class UserService(ApplicationDbContext context) : IUserService
    {
        private readonly ApplicationDbContext _context = context;


        // ADD USER //
        public async Task AddUser(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user), "user is null!");

            await _context.AddAsync(user);
        }



        // GET USER //
        public async Task<User?> GetUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id), "id is null or empty!");

            return await _context.Users.FindAsync(id);
        }

        // UPDATE USER //
        public void UpdateUser(User user)
        {
            if (user == null) throw new ArgumentException(nameof(user), "user is null!");

            var local = _context.Users.Local.FirstOrDefault(p => p.Id == user.Id);

            if (local != null) _context.Entry(local).State = EntityState.Detached;

            _context.Update(user);
        }


        // REMOVE USER //
        public void RemoveUser(User user)
        {
            if (user == null) throw new ArgumentException(nameof(user), "user is null!");

            _context.Users.Remove(user);
        }


        // GET USERS LIST //
        public async Task<IEnumerable<User>> GetUsersList() => await _context.Users.ToListAsync();


        // ANY USER EXIST? //
        public bool AnyUserExists_Id(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id), "id is null or empty!");

            return _context.Users.Any(e => e.Id == id);
        }

        // ANY USER EXIST? //
        public bool AnyUserExists_UserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentNullException(nameof(userName), "userName is null or empty!");

            return _context.Users.Any(p => p.UserName == userName.ToUpper());
        }

        //// ANY USER EXIST? //
        //public bool AnyUserExists_UserName(string userName)
        //{
        //    if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentNullException(nameof(userName), "userName is null or empty!");

        //    return _context.Users.Any(p => p.UserName == userName.ToUpper());
        //}

        // SAVE CHANGES //
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }




        // ROLE //
        // ADD USER ROLE //
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



        // GET USER-ROLE LIST //
        public async Task<IEnumerable<IdentityUserRole<string>>> GetUserRole_List() => await _context.UserRoles.ToListAsync();

        // GET ROLE LIST //
        public async Task<IEnumerable<IdentityRole<string>>> GetRole_List() => await _context.Roles.ToListAsync();




    }
}
