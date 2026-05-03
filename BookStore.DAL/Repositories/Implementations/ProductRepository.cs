using Microsoft.EntityFrameworkCore;
using ShopNest.DAL.ApplicationDbContext;
using ShopNest.DAL.Repositories.Interfaces;
using ShpoNest.Models.Entities;


namespace ShopNest.DAL.Repositories.Implementations
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetAllWithCategoryAsync()
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Images)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
        {
            return await _dbSet
                .Include(p => p.Images)
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdWithImagesAsync(int id)
        {
            return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetLowStockAsync(int threshold = 10)
        {
            return await _dbSet
            .Include(p => p.Category)
            .Where(p => p.Stock <= threshold && p.IsActive)
            .OrderBy(p => p.Stock)
            .ToListAsync();
        }
        public async Task<bool> ISBNExistsAsync(string isbn)
            => await _dbSet.AnyAsync(p => p.ISBN == isbn);

        public async Task<Product?> GetByISBNAsync(string isbn)
            => await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.ISBN == isbn);
    }
}
