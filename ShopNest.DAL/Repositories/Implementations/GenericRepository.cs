using Microsoft.EntityFrameworkCore;
using ShopNest.DAL.ApplicationDbContext;
using ShopNest.DAL.Repositories.Interfaces;


namespace ShopNest.DAL.Repositories.Implementations
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public async Task<int> CountAsync() => await _dbSet.CountAsync();

        public async Task<bool> ExistsAsync(int id) => await _dbSet.FindAsync(id) != null;
    }
}
