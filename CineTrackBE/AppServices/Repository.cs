using CineTrackBE.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace CineTrackBE.AppServices
{
    public interface IRepository<T> where T : class
    {
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task<T?> GetAsync(string id, CancellationToken cancellationToken = default);
        Task<T?> GetAsync(int id, CancellationToken cancellationToken = default);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        void Update(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        IQueryable<T> GetList();
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    }





    public class Repository<T>(ApplicationDbContext context, ILogger<Repository<T>> logger) : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<Repository<T>> _logger = logger;



        // ADD NEW ENTITY //
        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity);

            await _context.AddAsync(entity, cancellationToken);
        }


        // ADD-RANGE NEW ENTITY //
        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entities);
            if (!entities.Any()) throw new ArgumentException("The collection is empty.", nameof(entities));

            await _context.AddRangeAsync(entities, cancellationToken);
        }


        // GET ASYNC ENTITY - id string //
        public async Task<T?> GetAsync(string id, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);

            var entity = await _context.Set<T>().FindAsync([id], cancellationToken);

            if (entity == null)
            {
                _logger.LogWarning("Entity of type {EntityType} with ID {EntityId} not found.", typeof(T).Name, id);
            }

            return entity;
        }



        // GET ASYNC ENTITY - id int //
        public async Task<T?> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Set<T>().FindAsync([id], cancellationToken);

            if (entity == null)
            {
                _logger.LogWarning("Entity of type {EntityType} with ID {EntityId} not found.", typeof(T).Name, id);
            }

            return entity;
        }

        // FIRST OR DEFAULT ASYNC //
        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            return await _context.Set<T>().FirstOrDefaultAsync(predicate, cancellationToken);
        }

        // UPDATE ETITY //
        public void Update(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            _context.Set<T>().Update(entity);
        }


        // REMOVE ENTITY //
        public void Remove(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            _context.Set<T>().Remove(entity);
        }


        // REMOVE RANGE ENTITY //
        public void RemoveRange(IEnumerable<T> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);
            if (!entities.Any()) throw new ArgumentException("The collection is empty.", nameof(entities));

            _context.Set<T>().RemoveRange(entities);
        }


        // GET LIST //
        public IQueryable<T> GetList()
        {
            return _context.Set<T>().AsQueryable();
        }

        // GET ALL ASYNC //
        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().ToListAsync();
        }


        // ANY ASYNC //
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            return await _context.Set<T>().AnyAsync(predicate, cancellationToken);
        }

        // HAVE COUNT //
        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().CountAsync(cancellationToken);
        }


        // SAVE CHANGES //
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to save changes to database");
                throw new ArgumentException(ex.Message);
            }
        }

        // BEGIN TRANSACTION ASYNC //
        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Database.BeginTransactionAsync(cancellationToken);
        }

    }
}
