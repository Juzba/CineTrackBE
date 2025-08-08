using CineTrackBE.Data;
using CineTrackBE.Models;
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
        bool AnyUserExists_Email(string email);
        Task SaveChangesAsync();




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
        public bool AnyUserExists_Email(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentNullException(nameof(email), "email is null or empty!");

            return _context.Users.Any(p => p.NormalizedEmail == email.ToUpper());
        }

        // SAVE CHANGES //
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}
