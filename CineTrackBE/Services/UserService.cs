using CineTrackBE.Data;
using CineTrackBE.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.Services
{
    public interface IUserService
    {
        Task<bool> AnyUserExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    }


    public class UserService(ApplicationDbContext context) : IUserService
    {
        private readonly ApplicationDbContext _context = context;


        //// UPDATE USER //
        //public async Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
        //{
        //    if (user == null) throw new ArgumentException(nameof(user), "user is null!");

        //    var local = _context.Users.Local.FirstOrDefault(p => p.Id == user.Id);

        //    if (local != null) _context.Entry(local).State = EntityState.Detached;

        //    _context.Update(user);
        //    await _context.SaveChangesAsync(cancellationToken);
        //}


        //// REMOVE USER //
        //public async Task RemoveUserAsync(User user, CancellationToken cancellationToken = default)
        //{
        //    if (user == null) throw new ArgumentException(nameof(user), "user is null!");

        //    _context.Users.Remove(user);
        //}


        //// GET USERS LIST //
        //public async Task<IQueryable<User>> GetUsersListAsync(CancellationToken cancellationToken = default) => await Task.FromResult(_context.Users.AsQueryable());


        //// ANY USER EXIST? //
        //public async Task<bool> AnyUserExistsAsync(string id, CancellationToken cancellationToken = default)
        //{
        //    if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id), "id is null or empty!");

        //    return await _context.Users.AnyAsync(e => e.Id == id, cancellationToken);
        //}


        // ANY USER EXIST? //
        public async Task<bool> AnyUserExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentNullException(nameof(userName), "userName is null or empty!");

            return await _context.Users.AnyAsync(p => p.UserName == userName.ToUpper(), cancellationToken);
        }


  
    }
}
