using CineTrackBE.Data;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.AppServices
{
    public interface IRepository<T> where T : class
    {
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        Task<T?> GetAsync_Id(string id, CancellationToken cancellationToken = default);
        Task<T?> GetAsync_Id(int id, CancellationToken cancellationToken = default);
        void Update(T entity);
        void Remove(T entity);
        IQueryable<T> GetList();
        Task<bool> AnyExistsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AnyExistsAsync(string id, CancellationToken cancellationToken = default);

        Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
    }





    public class Repository<T>(ApplicationDbContext context) : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context = context;



        // ADD NEW ENTITY //
        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity);

            await _context.AddAsync(entity, cancellationToken);
        }


        // GET ENTITY - id string //
        public async Task<T?> GetAsync_Id(string id, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);

            return await _context.Set<T>().FindAsync([id], cancellationToken);
        }


        // GET ENTITY - id int //
        public async Task<T?> GetAsync_Id(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().FindAsync([id], cancellationToken);
        }


        // UPDATE ETITY //
        public void Update(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var entry = _context.Entry(entity);

            if (entry.State == EntityState.Detached)
            {
                _context.Attach(entity);
            }
            _context.Entry(entity).State = EntityState.Modified;
        }


        // REMOVE ENTITY //
        public void Remove(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            _context.Set<T>().Remove(entity);
        }




        // GET LIST //
        public IQueryable<T> GetList()
        {
            return _context.Set<T>().AsQueryable();
        }


        // ANY ENTITY EXIST? - id string //
        public async Task<bool> AnyExistsAsync(string id, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);

            return await _context.Set<T>().FindAsync([id], cancellationToken) != null;
        }


        // ANY ENTITY EXIST? - id int //
        public async Task<bool> AnyExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().FindAsync([id], cancellationToken) != null;
        }






        // SAVE CHANGES //
        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken) > 0;
            }
            catch (DbUpdateException)
            {
                //throw new ArgumentException(ex.Message);
                return false;
            }
        }
    }
}
